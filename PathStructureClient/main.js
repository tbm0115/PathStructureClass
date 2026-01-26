const { app, BrowserWindow, Menu, Tray, ipcMain, nativeImage } = require('electron');
const { execFile, spawn } = require('child_process');
const fs = require('fs');
const net = require('net');
const path = require('path');

const watcherPort = Number.parseInt(process.env.PATHSTRUCTURE_WATCHER_PORT || '49321', 10);
const watcherProcessName = 'PathStructure.WatcherHost.exe';

let mainWindow;
let tray;
let watcherProcess;
let watcherSocket;
let watcherBuffer = '';
let reconnectTimer;

const createWindow = () => {
  const win = new BrowserWindow({
    width: 900,
    height: 650,
    alwaysOnTop: true,
    show: false,
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
    sendWatcherStatus({
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
    sendWatcherStatus({
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
  if (watcherSocket) {
    watcherSocket.destroy();
    watcherSocket = null;
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

const sendWatcherStatus = (status) => {
  if (mainWindow && mainWindow.webContents) {
    mainWindow.webContents.send('watcher-status', status);
  }
};

const connectToWatcherHost = () => {
  if (watcherSocket) {
    return;
  }
  watcherSocket = net.createConnection({ host: '127.0.0.1', port: watcherPort }, () => {
    sendWatcherStatus({
      connected: true,
      message: `Connected to watcher host on port ${watcherPort}.`
    });
  });

  watcherSocket.on('data', (chunk) => {
    watcherBuffer += chunk.toString();
    const payloads = watcherBuffer.split('\n');
    watcherBuffer = payloads.pop() || '';
    payloads.filter(Boolean).forEach((payload) => {
      try {
        const parsed = JSON.parse(payload);
        if (mainWindow && mainWindow.webContents) {
          mainWindow.webContents.send('watcher-event', parsed);
        }
      } catch (error) {
        sendWatcherStatus({
          connected: true,
          message: 'Received malformed watcher payload.'
        });
      }
    });
  });

  watcherSocket.on('close', () => {
    watcherSocket = null;
    watcherBuffer = '';
    sendWatcherStatus({ connected: false, message: 'Watcher host disconnected.' });
    scheduleWatcherReconnect();
  });

  watcherSocket.on('error', () => {
    watcherSocket = null;
    watcherBuffer = '';
    sendWatcherStatus({ connected: false, message: 'Unable to reach watcher host.' });
    scheduleWatcherReconnect();
  });
};

const bootWatcherHost = async () => {
  await stopExistingWatcherHost();
  startWatcherHost();
  connectToWatcherHost();
};

app.whenReady().then(async () => {
  mainWindow = createWindow();
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

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('before-quit', () => {
  app.isQuitting = true;
  stopWatcherHost();
});
