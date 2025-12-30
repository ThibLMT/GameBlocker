# 🛡️ GameBlocker

GameBlocker is an enterprise-grade Windows Service designed to combat procrastination by detecting and terminating game processes. It features a hybrid architecture combining a high-performance C# background daemon with a modern React dashboard.

## 🚀 Project Status: Beta

Transitioned from a simple CLI script to a full System Service architecture.

- [x] Daemon Core: Background Service with graceful shutdown and thread-safe concurrency.

- [x] Smart Detection: Recursive directory scanning with heuristic grouping (Steam/Standalone).

- [x] Noise Filtering: Intelligent filtering of crash reporters, uninstallers, and tools.

- [x] Web Dashboard: React + TypeScript UI served directly by the Backend (Kestrel).

- [x] Hot Reload: Dynamic configuration updates without service restarts.

- [ ]  Persistence: Saving user-selected rules to disk (Current Sprint).

- [ ] Desktop Wrapper: Native WebView2 Launcher (In Progress).

## 🛠️ Architecture & Stack

### Backend (.NET 10)

- Microsoft.Extensions.Hosting: Generic Host for dependency injection and lifecycle management.
- ASP.NET Core (Kestrel): Embedded HTTP API to serve the UI and handle control requests.
- Serilog: Structured file logging for headless operation.
- System.IO: High-performance file enumeration and path manipulation.

### Frontend (React)
- Vite: High-speed build tool.
- Tailwind CSS (v4): Modern styling.
- Polling: Real-time state synchronization with the backend.

## 📦 Build & Distribution Strategy

The application is architected to run as a single unit: the C# Service acts as the file server for the compiled React frontend.

### 1. Build Frontend

Compile the TypeScript/React code into static assets.
```Powershell
cd GameBlocker.UI
npm install
npm run build
# Output: GameBlocker.UI/dist/
```
### 2. Embed into Backend

Copy the build artifacts into the Service's web root.

```Powershell
# Create directory if missing
mkdir ..\GameBlocker\GameBlocker\wwwroot

# Copy files
Copy-Item -Recurse -Force .\dist\* ..\GameBlocker\GameBlocker\wwwroot\
```

### 3. Publish Service

Compile the C# Backend into a standalone production executable.

```Powershell
cd ..\GameBlocker\GameBlocker
dotnet publish -c Release -o ./publish -p:PublishSingleFile=true --self-contained true
```

### 4. Install & Run

To deploy, create a Zip containing GameBlocker.exe, appsettings.json, and the wwwroot folder.

Installation (Admin PowerShell):
```Powershell
sc.exe create "GameBlocker" binPath= "C:\Path\To\GameBlocker.exe" start= auto
sc.exe start "GameBlocker"
```

Access Dashboard: Open `http://localhost:5000` in your browser.

## 🧪 Development

    Backend: dotnet run (Runs on port 5000).

    Frontend: npm run dev (Runs on port 5173 with HMR).

    Tests: dotnet test (xUnit + Moq).