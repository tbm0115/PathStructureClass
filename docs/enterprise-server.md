# PathStructure Enterprise Server

PathStructure supports two installation paths:

- **Personal Install**: A user installs the PathStructure bundle (PathStructureClient + PathStructure.WatcherHost) and manages path structure JSON locally (saved under `%LOCALAPPDATA%`).
- **Enterprise Install**: A server admin deploys or approves the PathStructure bundle and centrally governs configurations, authorization, and telemetry through the enterprise server.

The enterprise server is the central authority for managed installations. It exposes REST endpoints for admin configuration, client registration, usage reporting, and path model management.

## Responsibilities

- Verify managed client installations against configured authorization policies (Microsoft Entra or LDAP).
- Provide a centralized configuration store for enterprise management settings and installation profiles.
- Receive usage telemetry from managed clients.
- Store reusable path structure models and track the active model assignment.
- Keep enterprise controls centralized so client-side watcher hosts only manage local path structure actions.

## Running the server

Build and run the ASP.NET Core service:

```bash
dotnet run --project PathStructure.Server
```

By default the server stores its configuration at `%LOCALAPPDATA%\PathStructure\pathstructure-enterprise.json`.

The Razor-based admin UI is available at `/admin/` and renders the same data shown by the REST APIs.

## REST API overview

| Method | Route | Description |
| --- | --- | --- |
| GET | `/api/health` | Health check. |
| GET | `/api/management/config` | Fetch enterprise management settings. |
| PUT | `/api/management/config` | Update enterprise management settings. |
| POST | `/api/clients/register` | Register a client device. |
| POST | `/api/clients/{clientId}/usage` | Submit a client usage report. |
| GET | `/api/clients` | List registered clients. |
| GET | `/api/models` | List reusable path structure models. |
| POST | `/api/models` | Create or update a path structure model. |
| POST | `/api/models/{modelId}/apply` | Mark a model as the active assignment. |
| GET | `/api/installation/profiles` | List installation profiles. |

## Sample payloads

Update management settings:

```json
{
  "management": {
    "authorization": {
      "mode": "entra",
      "allowedClientIds": ["desktop-01"],
      "allowedPrincipals": ["alex@contoso.com"],
      "entra": {
        "tenantId": "tenant-id",
        "allowedGroups": ["group-id"]
      }
    },
    "usageReporting": {
      "required": true,
      "minimumReportIntervalSeconds": 120
    },
    "installation": {
      "defaultProfileId": "default"
    }
  }
}
```

Register a client:

```json
{
  "clientId": "desktop-01",
  "deviceName": "DESKTOP-01",
  "userName": "alex",
  "principal": "alex@contoso.com",
  "provider": "entra",
  "installProfileId": "default"
}
```

Create a path structure model:

```json
{
  "model": {
    "name": "Finance",
    "description": "Finance folder structure",
    "paths": [
      { "regex": "^Finance$", "name": "Finance" }
    ]
  }
}
```

## LDAP provisioning guidance

1. **Configure LDAP settings in the enterprise server** via the management API:

```json
{
  "management": {
    "authorization": {
      "mode": "ldap",
      "allowedPrincipals": ["CN=PathStructureUsers,OU=Groups,DC=example,DC=com"],
      "ldap": {
        "host": "ldap.example.com",
        "port": 636,
        "useSsl": true,
        "baseDn": "DC=example,DC=com",
        "allowedGroups": ["CN=PathStructureUsers,OU=Groups,DC=example,DC=com"]
      }
    }
  }
}
```

2. **Provision client app settings** so the client knows where to register:

Store a JSON file such as `%LOCALAPPDATA%\PathStructure\clientsettings.json`:

```json
{
  "enterpriseServerUrl": "https://server.example.com",
  "clientId": "desktop-01",
  "deviceName": "DESKTOP-01",
  "principal": "CN=Alex Smith,OU=Users,DC=example,DC=com"
}
```

3. **Server-managed path structures** should be referenced as import URLs in the local path structure config. For example, in the client's local config:

```json
{
  "imports": [
    { "path": "https://server.example.com/api/models/finance/pathstructure.json" }
  ]
}
```

Client applications should treat URL-based imports as read-only (no local edits), while local file-based imports remain editable by the user.
