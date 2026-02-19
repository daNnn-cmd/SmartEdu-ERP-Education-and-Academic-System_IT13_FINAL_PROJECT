# SmartEduERP

SmartEduERP is a .NET MAUI Blazor Hybrid Education & Academic Management System.

It provides role-based access for Admin, Teacher, and Student accounts and includes modules for student/teacher management, subjects, enrollments/approvals, grades, payments, audit logging, exports, and database synchronization.

## Features

- **Authentication & authorization**
  - Login using **email or username**
  - Registration + email confirmation flow
  - Route protection (only Home/Login/Register are public)

- **Core academic modules**
  - Student management (CRUD + soft delete)
  - Teacher management (CRUD + soft delete)
  - Subject management
  - Enrollment workflow with **Pending / Approved / Rejected** status
  - Grades and Payments

- **Audit logging**
  - Authentication events (login/logout/register)
  - CRUD events (create/update/delete) with old/new values

- **Exports**
  - Export teacher lists to Excel/PDF with generated timestamps

- **Sync / Offline-first workflow**
  - Bidirectional database sync (local \u2194 cloud)
  - Sync scheduling + retry
  - Optional sync trigger when WiFi connects

## Tech Stack

- **.NET MAUI Blazor Hybrid**
- **EF Core** (local + cloud SQL Server)
- **Windows target** (net9.0-windows)

## Prerequisites

- **Windows 10/11**
- **.NET SDK** installed (project is currently using .NET 9 on Windows)
- Visual Studio 2022 with **.NET MAUI** workload (recommended)
- SQL Server (for local database)

## Getting Started

### 1) Clone

```bash
git clone https://github.com/daNnn-cmd/SmartEdu-ERP-Education-and-Academic-System_IT13_FINAL_PROJECT.git
cd SmartEdu-ERP-Education-and-Academic-System_IT13_FINAL_PROJECT
```

### 2) Configure settings

Update `SmartEduERP/appsettings.json`:

- `ConnectionStrings:DefaultConnection` (your local SQL Server)
- `ConnectionStrings:CloudConnection` (optional cloud DB)
- `DatabaseSync` settings (optional)

> Security note: Do not commit real production credentials. Prefer user-secrets or environment variables for sensitive values.

### 3) Restore + Build

```bash
dotnet restore
dotnet build SmartEduERP.sln
```

### 4) Run

In Visual Studio:
- Set the startup project to `SmartEduERP`
- Select **Windows Machine** and run

Or via CLI (Windows):

```bash
dotnet run --project SmartEduERP/SmartEduERP.csproj
```

## Configuration Reference

### Database Sync (`DatabaseSync`)

Example:

```json
"DatabaseSync": {
  "SyncEnabled": true,
  "SyncIntervalMinutes": 5,
  "SyncOnStartup": true,
  "SyncDirection": "Bidirectional",
  "SyncOnWiFiConnect": true,
  "ConnectivityCheckIntervalSeconds": 10,
  "RetryAttempts": 3,
  "RetryIntervalSeconds": 30
}
```

## Repository Notes

- This repo includes a `.gitignore` to avoid committing IDE and build artifacts.
- If you add new secrets/keys, store them outside git history.

## License

For academic use (school project). Add a license file if you plan to distribute this publicly.
