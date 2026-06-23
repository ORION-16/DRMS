# DRMS - Deployment Request Management System

DRMS is an ASP.NET Core MVC application for managing deployment requests through a role-based approval workflow. It helps developers submit deployment requests, routes them through Tech Lead, QA, and DevOps approvals, and gives administrators a consolidated view of request status and deployment history.

## Features

- Cookie-based authentication with role-aware redirects.
- Developer dashboard for request tracking and deployment summaries.
- Deployment request creation with project, environment, deployment type, rollback plan, and target date details.
- Multi-step approval workflow for Tech Lead, QA, and DevOps users.
- Admin request listing with status and project filters.
- Deployment history tracking.
- SQL Server persistence through Dapper and stored procedures.

## Tech Stack

- .NET 10
- ASP.NET Core MVC
- C#
- SQL Server
- Dapper
- BCrypt.Net-Next
- Bootstrap, jQuery, and Razor views

## Solution Structure

```text
DRMS/
├── DRMS.Domain/          # Entities and repository contracts
├── DRMS.Application/     # DTOs and application services
├── DRMS.Infrastructure/  # Dapper context and repository implementations
├── DRMS.Web/             # ASP.NET Core MVC app, controllers, views, static assets
├── database/             # Database schema, seed data, stored procedures, SQL tests
├── docs/                 # Workflow and supporting documentation
└── DRMS.slnx             # Solution file
```

## Architecture

The project follows a layered structure:

- `DRMS.Domain` defines the core entities and repository interfaces.
- `DRMS.Application` contains DTOs and service-level business flow.
- `DRMS.Infrastructure` implements data access using Dapper and SQL Server stored procedures.
- `DRMS.Web` hosts the MVC controllers, Razor views, authentication, authorization policies, and dependency injection setup.

## Prerequisites

- .NET 10 SDK
- SQL Server running locally or remotely
- A SQL client such as SQL Server Management Studio, Azure Data Studio, or `sqlcmd`

## Database Setup

Run the SQL scripts in this order:

1. `database/01_schema_and_seed.sql`
2. `database/02_stored_procedures.sql`
3. Optional: `database/04_update_test_password_hash.sql`
4. Optional validation: `database/03_test_stored_procedures.sql`

The schema script creates the `DRMS` database, master tables, transaction tables, roles, workflow rows, sample users, and sample projects. The stored procedure script creates the procedures used by the repositories.

## Configuration

Do not commit real database passwords. For local development, copy the example file and update the password:

```bash
cp DRMS.Web/appsettings.Development.example.json DRMS.Web/appsettings.Development.json
```

Then edit:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=DRMS;User Id=sa;Password=YOUR_LOCAL_PASSWORD;TrustServerCertificate=True;"
  }
}
```

You can also use user secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=DRMS;User Id=sa;Password=YOUR_LOCAL_PASSWORD;TrustServerCertificate=True;" --project DRMS.Web
```

## Run Locally

Restore and build the solution:

```bash
dotnet restore DRMS.slnx
dotnet build DRMS.slnx
```

Start the web app:

```bash
dotnet run --project DRMS.Web
```

Open the URL printed by the `dotnet run` command. The default route is the login page.

## User Roles

DRMS uses the following roles:

- `Developer` - creates and tracks deployment requests.
- `TechLead` - performs the first approval step.
- `QA` - performs the quality approval step.
- `DevOps` - performs the final deployment approval step.
- `Admin` - reviews all requests and deployment history.

## Useful Commands

```bash
dotnet restore DRMS.slnx
dotnet build DRMS.slnx
dotnet run --project DRMS.Web
```

## Repository Hygiene

Generated build folders such as `bin/` and `obj/` are ignored through `.gitignore`. Local configuration files such as `appsettings.Development.json` are also ignored so credentials stay out of the repository.

Before pushing to GitHub:

```bash
git init
git add .
git status
git commit -m "Initial DRMS project"
```
