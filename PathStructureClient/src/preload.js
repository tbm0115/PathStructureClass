const { contextBridge, ipcRenderer } = require('electron');

/**
 * @typedef {object} PathStructureBridge
 * @property {string} platform
 * @property {(callback: (payload: unknown) => void) => void} onStatus
 * @property {(callback: (payload: unknown) => void) => void} onPathUpdate
 * @property {(method: string, params?: Record<string, unknown>) => Promise<unknown>} sendJsonRpcRequest
 * @property {() => Promise<unknown>} scaffoldRequiredFolders
 * @property {() => Promise<void>} showWindow
 * @property {() => Promise<void>} startService
 * @property {() => Promise<void>} stopService
 * @property {() => Promise<void>} softReset
 * @property {() => Promise<void>} openAddPathWindow
 * @property {() => Promise<void>} openImportManagerWindow
 * @property {(index: number) => Promise<void>} selectMatchIndex
 * @property {(url: string) => Promise<void>} importUrl
 * @property {(status: unknown) => Promise<void>} notifyStatus
 */

/** @type {PathStructureBridge} */
const bridge = {
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
  openImportManagerWindow: () => ipcRenderer.invoke('open-import-manager-window'),
  selectMatchIndex: (index) => ipcRenderer.invoke('select-match-index', index),
  importUrl: (url) => ipcRenderer.invoke('import-url', url),
  notifyStatus: (status) => ipcRenderer.invoke('client-status', status)
};

contextBridge.exposeInMainWorld('pathStructure', bridge);
