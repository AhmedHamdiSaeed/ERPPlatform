# ERPPlatform

Enterprise SaaS ERP Platform built with ABP Framework, .NET 9, Angular 20, and OpenIddict.

---

## 🚀 Quick Start Guide (Run from Terminal / CLI)

### Pre-requisites
* [.NET 9.0+ SDK](https://dotnet.microsoft.com/download/dotnet)
* [Node.js v20.11+](https://nodejs.org/en)

---

### Step 1: Initialize Database & Seed Demo Data
Run the database migrator from the root directory to create the database, apply EF migrations, and seed initial demo accounts (including Admin roles and mobile phone numbers):

```bash
dotnet run --project Shared/ERPPlatform.DbMigrator/ERPPlatform.DbMigrator.csproj
```

---

### Step 2: Run Backend API Host

#### Standard Terminal Command
```bash
dotnet run --project Host/ERPPlatform.HttpApi.Host/ERPPlatform.HttpApi.Host.csproj
```

#### Hot Reload Mode (Recommended for Active Backend Dev)
Auto-reloads backend endpoints when C# files are modified:
```bash
dotnet watch --project Host/ERPPlatform.HttpApi.Host/ERPPlatform.HttpApi.Host.csproj
```

- **Swagger UI**: `https://localhost:44327/swagger`
- **Token Endpoint**: `https://localhost:44327/connect/token`

---

### Step 3: Run Angular Frontend

Open a new terminal window:

```bash
cd angular
npm install
npm start
```

- **Frontend App**: `http://localhost:4200`

---

## 🐳 Docker Quick Start (Recommended)

Run the entire platform with a single command — no need to install .NET SDK, Node.js, or SQL Server on your machine.

### Pre-requisites
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running

---

### 🧪 Testing / Development Mode (Local Database)

Spins up a local SQL Server container with a fresh database:

```bash
docker compose up --build
```

This will:
1. Start a **SQL Server 2022** container with a local database
2. Build and start the **.NET 9 Backend API** (with auto DB migration)
3. Build and start the **Angular 20 Frontend** served via Nginx

| Service | URL |
| :--- | :--- |
| **Frontend App** | `http://localhost:4200` |
| **Backend API (Swagger)** | `http://localhost:8080/swagger` |
| **SQL Server** | `localhost:1433` (User: `sa` / Password: `ERPPlatform_Dev123!`) |

```bash
# Stop (keeps database data)
docker compose down

# Stop and DELETE database (fresh start)
docker compose down -v
```

---

### 🚀 Production Mode (Remote Database)

Connects to the remote production database — no local SQL Server container needed:

```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml up --build
```

```bash
# Stop production containers
docker compose -f docker-compose.yml -f docker-compose.prod.yml down
```

| Service | URL |
| :--- | :--- |
| **Frontend App** | `http://localhost:4200` |
| **Backend API (Swagger)** | `http://localhost:8080/swagger` |

---

### Build Individual Containers

```bash
# Build only the backend
docker compose build backend

# Build only the frontend
docker compose build frontend

# Rebuild everything from scratch (no cache)
docker compose build --no-cache
```

---

## 🔑 Demo & Admin Accounts

You can log in using **Email Address**, **Username**, or **Mobile Phone Number**:

| Login Format | Account Input | Password | Assigned Role |
| :--- | :--- | :--- | :--- |
| **Email (System Admin)** | `admin@erpplatform.com` | `Admin123!` | Admin |
| **Email (Admin)** | `ahmed.hamdi@erpplatform.com` | `Admin123!` | Admin |
| **Username** | `admin` | `Admin123!` | Admin |
| **Mobile Phone** | `+201000000000` | `Admin123!` | Admin |
| **Mobile Phone (Alt)** | `01000000000` | `Admin123!` | Admin |
| **HR Manager** | `sara.mansour@erpplatform.com` | `Manager123!` | HR Manager |
| **Employee** | `omar.khaled@erpplatform.com` | `Employee123!` | Employee |

---

## 📱 Mobile App Development Notes

For mobile developers connecting from emulator or physical devices:
- **Android Emulator**: Set API base URL to `https://10.0.2.2:44327` or `http://10.0.2.2:44327`
- **iOS Simulator**: Set API base URL to `https://localhost:44327`
- **Physical Mobile Device on Wi-Fi**: Run backend binding to all interfaces:
  ```bash
  dotnet run --project Host/ERPPlatform.HttpApi.Host/ERPPlatform.HttpApi.Host.csproj --urls "http://0.0.0.0:44327"
  ```
  And set mobile API base URL to `http://<your-laptop-ip>:44327`.

---

## 📁 Solution Structure

- `Shared/ERPPlatform.DbMigrator`: Applies database migrations and seeds initial accounts.
- `Host/ERPPlatform.HttpApi.Host`: ASP.NET Core REST API host and OpenIddict server.
- `angular`: Angular 20 SPA frontend application.
- `Modules`: Modular architecture domain, application, and infrastructure layers (HR, Inventory, Workflow, AI).
