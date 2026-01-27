const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('pathStructure', {
  platform: process.platform,
  onStatus: (callback) => ipcRenderer.on('pathstructure-status', (_event, payload) => callback(payload)),
  onPathUpdate: (callback) => ipcRenderer.on('pathstructure-update', (_event, payload) => callback(payload)),
  sendJsonRpcRequest: (method, params) => ipcRenderer.invoke('json-rpc-request', { method, params }),
  scaffoldRequiredFolders: () => ipcRenderer.invoke('scaffold-required-folders'),
  showWindow: () => ipcRenderer.invoke('show-window'),
  startService: () => ipcRenderer.invoke('watcher-start'),
  stopService: () => ipcRenderer.invoke('watcher-stop'),
  softReset: () => ipcRenderer.invoke('soft-reset'),
  openAddPathWindow: () => ipcRenderer.invoke('open-add-path-window'),
  notifyStatus: (status) => ipcRenderer.invoke('client-status', status)
});
