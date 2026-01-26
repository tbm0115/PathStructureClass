const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('pathStructure', {
  platform: process.platform,
  onWatcherEvent: (callback) => ipcRenderer.on('watcher-event', (_event, payload) => callback(payload)),
  onWatcherStatus: (callback) => ipcRenderer.on('watcher-status', (_event, payload) => callback(payload)),
  showWindow: () => ipcRenderer.invoke('show-window')
});
