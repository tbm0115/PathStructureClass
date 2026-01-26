# PathStructureClient

PathStructureClient is the Electron shell for the PathStructure ecosystem. It launches a companion background process (`PathStructure.WatcherHost`) that runs the `ExplorerWatcher` logic and streams events over localhost TCP to the Electron UI.

## Background watcher host

The watcher host lives in the `PathStructure.WatcherHost` project. It listens on a localhost TCP port (default `49321`) and emits newline-delimited JSON events such as:

```json
{"type":"pathChanged","message":"Explorer path changed.","path":"C:\\Some\\Folder","timestamp":"2024-01-01T00:00:00.0000000+00:00"}
```

The Electron app starts the watcher host on launch and attempts to shut down lingering instances before starting a new one.

### Build the watcher host

From the repository root:

```powershell
msbuild PathStructure.sln /t:Build /p:Configuration=Release
```

The output binary is expected at:

```
PathStructure.WatcherHost\bin\Release\PathStructure.WatcherHost.exe
```

## Packaging an installer

The simplest way to build a Windows installer for the Electron app is to use `electron-builder`. A typical workflow looks like:

1. Install dependencies:
   ```powershell
   cd PathStructureClient
   npm install
   npm install --save-dev electron-builder
   ```

2. Add the watcher host output to your app resources (example `package.json` configuration):
   ```json
   {
     "build": {
       "appId": "com.pathstructure.client",
       "productName": "PathStructureClient",
       "files": [
         "**/*"
       ],
       "extraResources": [
         {
           "from": "../PathStructure.WatcherHost/bin/Release",
           "to": "watcher",
           "filter": ["PathStructure.WatcherHost.exe"]
         }
       ],
       "win": {
         "target": "nsis"
       }
     }
   }
   ```

3. Build the installer:
   ```powershell
   npm run build
   ```

### Optional: service-style packaging

If you want the watcher host installed alongside the client without running as a Windows Service:

- Keep it in the app bundle using `extraResources` as shown above.
- The Electron app starts/stops the process, so no Service Control Manager registration is needed.
- Update the `PATHSTRUCTURE_WATCHER_PORT` environment variable if you need a non-default port.

## Local development tips

- Run the Electron app:
  ```powershell
  npm start
  ```
- Ensure the watcher host is built (`Debug` output) so the Electron app can start it from `PathStructure.WatcherHost\bin\Debug`.
