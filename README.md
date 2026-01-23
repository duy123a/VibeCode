# VibeCode

A .NET 9.0 template project implementing Clean Architecture with OpenIddict authentication and page-based authorization.

## Overview

VibeCode serves as a template and training ground for future ASP.NET Core projects, demonstrating both simple login (Identity) and OAuth 2.0 with PKCE (OpenIddict) approaches. It provides reusable patterns for authentication, authorization, and data access.

## Tech Stack

- **.NET 9.0** (target framework)
- **ASP.NET Core MVC** (Main app)
- **OpenIddict** + **ASP.NET Core Identity** (IdentityServer)
- **Entity Framework Core** + **SQL Server**
- **Repository pattern** + **Unit of Work**
- **Policy-based authorization**

## Project Structure

```
VibeCode/
├── VibeCode.IdentityServer/    # Auth server (OpenIddict + Identity)
├── VibeCode.Main/              # MVC client application
└── VibeCode.Shared/            # Shared entities, interfaces, DTOs, validation, resources
```

### Key Directories

- `Shared/Entities` - Domain models (AppUser, AppRole, Page, Permission, ISoftDeletable, IAuditable)
- `Shared/Interfaces` - IRepository, IService, IUnitOfWork
- `Shared/Repositories` - EF Core repository implementations
- `Shared/Models` - DTOs, ViewModels
- `Shared/Validation` - Custom validation attributes
- `Shared/Resources` - Multi-language resource files

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- SQL Server
- Git

### Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/duy123a/VibeCode.git
   cd VibeCode
   ```

2. Configure connection strings in `appsettings.json` for both projects.

3. Run migrations:
   ```bash
   dotnet ef migrations add InitialSetup --project VibeCode.IdentityServer --startup-project VibeCode.IdentityServer
   dotnet ef migrations script --project VibeCode.IdentityServer
   dotnet ef migrations add InitialSetup --project VibeCode.Main --startup-project VibeCode.Main
   dotnet ef migrations script --project VibeCode.Main
   ```

4. Run the projects:
   ```bash
   dotnet run --project VibeCode.IdentityServer
   dotnet run --project VibeCode.Main
   ```

### Essential Commands

```bash
# Build
dotnet clean && dotnet build

# Run projects
dotnet run --project VibeCode.IdentityServer
dotnet run --project VibeCode.Main

# Generate migrations
dotnet ef migrations add <Name> --project VibeCode.IdentityServer --startup-project VibeCode.IdentityServer
dotnet ef migrations script --project VibeCode.IdentityServer

# Run tests
dotnet test
```

## Architecture

### Authentication

Two authentication approaches are supported:

1. **Simple Login** - ASP.NET Core Identity only (internal systems, quick setup)
2. **OAuth 2.0 with PKCE** - OpenIddict with full authorization code flow (production, multiple clients)

### Authorization

Current implementation uses role-based authorization:
```csharp
[Authorize(Roles = "Admin")]
public IActionResult AdminPage()
{
    // Only users with Admin role can access
}
```

Future: Page-based authorization with claims caching is planned.

### Data Access

- **AuthDbContext** (IdentityServer) - Authentication and authorization data
- **VibeCodeDbContext** (Main) - Business data management
- **Repository Pattern** - Interfaces in Shared, implementations in project-specific folders
- **Unit of Work** - Transaction management

## Documentation

| Documentation | Purpose |
|---------------|-----------|
| [AGENTS.md](AGENTS.md) | Project overview, commands, and implementation notes |
| [docs/architectural_patterns.md](docs/architectural_patterns.md) | Dependency injection, authorization patterns, data access conventions |
| [docs/dbcontext-architecture.md](docs/dbcontext-architecture.md) | Separate DbContexts strategy, migration patterns |
| [docs/permission-system-design-future.md](docs/permission-system-design-future.md) | Future implementation: Page-based authorization |
| [docs/code-samples/](docs/code-samples/) | Reference implementations for Identity, OpenIddict, and common patterns |

## Key Features

- **Separate DbContexts** for auth and business logic
- **Shared entities** in VibeCode.Shared referenced by both projects
- **Clear separation** of concerns and responsibilities
- **Microservices-ready** architecture
- **Independent migrations** for each project
- **Localization support** with resource files
- **Soft delete and audit trail** with ISoftDeletable and IAuditable interfaces
- **CORS configuration** for cross-origin requests
- **Custom model binders** for input validation (e.g., string trimming)

## License

This project is provided as a template for educational and development purposes.

## Contributing

This is a template project intended for training and reference. Feel free to fork and adapt it for your own projects.
