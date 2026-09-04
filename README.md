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
