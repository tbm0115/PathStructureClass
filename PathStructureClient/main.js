const { app, BrowserWindow, Menu, MenuItem, Tray, ipcMain, nativeImage } = require('electron');
const { execFile, spawn } = require('child_process');
const fs = require('fs');
const path = require('path');
const { JsonRpcService } = require('./src/services/jsonRpcService');
const { PathStructureService } = require('./src/services/pathStructureService');

const watcherPort = Number.parseInt(process.env.PATHSTRUCTURE_WATCHER_PORT || '49321', 10);
const watcherProcessName = 'PathStructure.WatcherHost.exe';

let mainWindow;
let addPathWindow;
let tray;
let watcherProcess;
let reconnectTimer;
let rpcService;
let pathStructureService;
let scaffoldMenuItem;

const createWindow = () => {
  const win = new BrowserWindow({
    width: 620,
    height: 490,
    alwaysOnTop: true,
    show: false,
    autoHideMenuBar: false,
    webPreferences: {
      contextIsolation: true,
      nodeIntegration: false,
      preload: path.join(__dirname, 'src', 'preload.js')
    }
  });

  win.loadFile(path.join(__dirname, 'src', 'index.html'));
  win.once('ready-to-show', () => win.show());

  win.on('minimize', (event) => {
    event.preventDefault();
    hideToTray();
  });

  win.on('close', (event) => {
    if (!app.isQuitting) {
      event.preventDefault();
      hideToTray();
    }
  });

  return win;
};

const createAddPathWindow = () => {
  if (addPathWindow) {
    addPathWindow.focus();
    return addPathWindow;
  }

  addPathWindow = new BrowserWindow({
    width: 420,
    height: 650,
    resizable: false,
    show: false,
    parent: mainWindow ?? undefined,
    modal: false,
    webPreferences: {
      contextIsolation: true,
      nodeIntegration: false,
      preload: path.join(__dirname, 'src', 'preload.js')
    }
  });

  addPathWindow.loadFile(path.join(__dirname, 'src', 'add-path.html'));
  addPathWindow.once('ready-to-show', () => addPathWindow.show());
  addPathWindow.on('closed', () => {
    addPathWindow = null;
  });

  return addPathWindow;
};

const hideToTray = () => {
  if (!mainWindow) {
    return;
  }
  mainWindow.hide();
  mainWindow.setSkipTaskbar(true);
};

const showFromTray = () => {
  if (!mainWindow) {
    return;
  }
  mainWindow.setSkipTaskbar(false);
  mainWindow.show();
  mainWindow.focus();
};

const createTray = async () => {
  if (tray) {
    return;
  }
  let trayIcon;
  try {
    trayIcon = await app.getFileIcon(process.execPath);
  } catch (error) {
    trayIcon = nativeImage.createEmpty();
  }

  tray = new Tray(trayIcon);
  tray.setToolTip('PathStructure Client');
  tray.on('click', () => showFromTray());

  const contextMenu = Menu.buildFromTemplate([
    { label: 'Show', click: () => showFromTray() },
    { type: 'separator' },
    {
      label: 'Quit',
      click: () => {
        app.isQuitting = true;
        app.quit();
      }
    }
  ]);

  tray.setContextMenu(contextMenu);
};

const createAppMenu = () => {
  scaffoldMenuItem = new MenuItem({
    label: 'Scaffold Folders',
    enabled: false,
    click: async () => {
      if (!pathStructureService) {
        return;
      }
      const result = await pathStructureService.scaffoldRequiredFolders();
      sendStatusUpdate({ connected: true, message: result.message });
    }
  });

  const template = [
    {
      label: 'Edit',
      submenu: [
        scaffoldMenuItem
      ]
    }
  ];

  const menu = Menu.buildFromTemplate(template);
  Menu.setApplicationMenu(menu);
};

const getWatcherExecutablePath = () => {
  if (app.isPackaged) {
    return path.join(process.resourcesPath, 'watcher', watcherProcessName);
  }
  return path.join(__dirname, '..', 'PathStructure.WatcherHost', 'bin', 'Debug', watcherProcessName);
};

const stopExistingWatcherHost = () => {
  if (process.platform !== 'win32') {
    return Promise.resolve();
  }
  return new Promise((resolve) => {
    execFile('taskkill', ['/F', '/IM', watcherProcessName, '/T'], () => resolve());
  });
};

const startWatcherHost = () => {
  const executablePath = getWatcherExecutablePath();
  if (!fs.existsSync(executablePath)) {
    sendStatusUpdate({
      connected: false,
      message: `Watcher host executable not found at ${executablePath}.`
    });
    return;
  }

  watcherProcess = spawn(executablePath, [String(watcherPort)], {
    windowsHide: true,
    stdio: 'ignore'
  });

  watcherProcess.on('error', () => {
    watcherProcess = null;
    sendStatusUpdate({
      connected: false,
      message: 'Failed to start watcher host.'
    });
    scheduleWatcherReconnect();
  });

  watcherProcess.on('exit', () => {
    watcherProcess = null;
    scheduleWatcherReconnect();
  });
};

const stopWatcherHost = () => {
  if (watcherProcess) {
    watcherProcess.kill();
    watcherProcess = null;
  }
  if (rpcService) {
    rpcService.disconnect();
  }
  if (reconnectTimer) {
    clearTimeout(reconnectTimer);
    reconnectTimer = null;
  }
};

const scheduleWatcherReconnect = () => {
  if (reconnectTimer) {
    return;
  }
  reconnectTimer = setTimeout(() => {
    reconnectTimer = null;
    connectToWatcherHost();
  }, 2000);
};

const sendStatusUpdate = (status) => {
  if (mainWindow && mainWindow.webContents) {
    mainWindow.webContents.send('pathstructure-status', status);
  }
};

const sendPathUpdate = (payload) => {
  if (mainWindow && mainWindow.webContents) {
    mainWindow.webContents.send('pathstructure-update', payload);
  }
  if (scaffoldMenuItem) {
    const children = payload?.children || [];
    scaffoldMenuItem.enabled = children.some((child) => child.isRequired && !child.isFile);
  }
};

const connectToWatcherHost = () => {
  if (!rpcService) {
    rpcService = new JsonRpcService({ host: '127.0.0.1', port: watcherPort });
  }
  if (!pathStructureService) {
    pathStructureService = new PathStructureService({ rpcService });
    pathStructureService.on('status', (status) => sendStatusUpdate(status));
    pathStructureService.on('update', (payload) => sendPathUpdate(payload));
  }

  rpcService.removeAllListeners('notification');
  rpcService.on('notification', (payload) => {
    void pathStructureService.handleNotification(payload);
  });

  rpcService.removeAllListeners('connected');
  rpcService.on('connected', () => {
    sendStatusUpdate({
      connected: true,
      message: `Connected to watcher host on port ${watcherPort}.`
    });
  });

  rpcService.removeAllListeners('disconnected');
  rpcService.on('disconnected', () => {
    sendStatusUpdate({ connected: false, message: 'Watcher host disconnected.' });
    scheduleWatcherReconnect();
  });

  rpcService.removeAllListeners('error');
  rpcService.on('error', () => {
    sendStatusUpdate({ connected: false, message: 'Unable to reach watcher host.' });
    scheduleWatcherReconnect();
  });

  rpcService.connect();
};

const bootWatcherHost = async () => {
  await stopExistingWatcherHost();
  startWatcherHost();
  connectToWatcherHost();
};

app.whenReady().then(async () => {
  mainWindow = createWindow();
  createAppMenu();
  await createTray();
  await bootWatcherHost();

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      mainWindow = createWindow();
    }
  });
});

ipcMain.handle('show-window', () => {
  showFromTray();
});

ipcMain.handle('json-rpc-request', (_event, payload) => {
  if (!rpcService) {
    return Promise.reject(new Error('JSON-RPC service not available.'));
  }
  return rpcService.sendRequest(payload?.method, payload?.params);
});

ipcMain.handle('scaffold-required-folders', async () => {
  if (!pathStructureService) {
    return { created: [], skipped: [], message: 'Path structure service not available.' };
  }
  const result = await pathStructureService.scaffoldRequiredFolders();
  sendStatusUpdate({ connected: true, message: result.message });
  return result;
});

ipcMain.handle('watcher-start', async () => {
  if (watcherProcess) {
    sendStatusUpdate({ connected: true, message: 'Watcher host already running.' });
    return;
  }
  await bootWatcherHost();
});

ipcMain.handle('watcher-stop', () => {
  stopWatcherHost();
  sendStatusUpdate({ connected: false, message: 'Watcher host stopped.' });
});

ipcMain.handle('soft-reset', () => {
  if (!rpcService) {
    return;
  }
  rpcService.disconnect();
  connectToWatcherHost();
});

ipcMain.handle('open-add-path-window', () => {
  createAddPathWindow();
});

ipcMain.handle('client-status', (_event, status) => {
  sendStatusUpdate(status);
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('before-quit', () => {
  app.isQuitting = true;
  stopWatcherHost();
});
