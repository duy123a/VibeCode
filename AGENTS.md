# AGENTS.md

## Project Overview

VibeCode is a .NET 9.0 template project implementing a Clean Architecture with OpenIddict authentication and page-based authorization.

### Purpose
- Template/training ground for future ASP.NET Core projects
- Demonstrates both simple login (Identity) and OAuth 2.0 with PKCE (OpenIddict) approaches
- Provides reusable patterns for authentication, authorization, and data access

### Tech Stack
- .NET 9.0 (target framework)
- ASP.NET Core MVC (Main app)
- OpenIddict + ASP.NET Core Identity (IdentityServer)
- Entity Framework Core + SQL Server
- Repository pattern + Unit of Work
- Policy-based authorization

### Project Structure
```
VibeCode.IdentityServer/    - Auth server (OpenIddict + Identity)
VibeCode.Main/             - MVC client application
VibeCode.Shared/            - Shared entities, interfaces, DTOs, validation, resources
```

**Key Directories:**
- `Shared/Entities` - Domain models (AppUser, AppRole, Page, Permission, ISoftDeletable, IAuditable)
- `Shared/Interfaces` - IRepository, IService, IUnitOfWork
- `Shared/Repositories` - EF Core repository implementations
- `Shared/Models` - DTOs, ViewModels
- `Shared/Validation` - Custom validation attributes
- `Shared/Resources` - Multi-language resource files

---

## Essential Commands

### Build
```bash
dotnet clean && dotnet build                    # Build entire solution
dotnet clean <Project> && dotnet build <Project>    # Build specific project
dotnet clean && dotnet build --configuration Release    # Release build
dotnet run --project VibeCode.IdentityServer       # Run IdentityServer
dotnet run --project VibeCode.Main                # Run Main app
```

### Test
```bash
dotnet test                                    # Run all tests
dotnet test --filter "FullyQualifiedName~ClassName"     # Run single test class
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"  # Run single method
```

### Database (IdentityServer)
```bash
dotnet ef migrations add <Name> --project VibeCode.IdentityServer --startup-project VibeCode.IdentityServer
dotnet ef migrations script --project VibeCode.IdentityServer
```

### Database (Main)
```bash
dotnet ef migrations add <Name> --project VibeCode.Main --startup-project VibeCode.Main
dotnet ef migrations script --project VibeCode.Main
```

**CRITICAL:** Never run `dotnet ef database update` without explicit approval. Review SQL scripts before execution.

---

## Architecture Decisions

- **Separate DbContexts:** AuthDbContext (IdentityServer) for auth, VibeCodeDbContext (Main) for business data
- **Shared Entities:** Both projects reference Shared for AppUser, AppRole, Page, Permission
- **Login Options:** Two approaches supported - see docs/code-samples/openiddict/login-options/
- **Admin Bypass:** Admin role automatically skips permission checks
- **Permission Caching:** Page permissions stored as claims, loaded on login
- **Repository Pattern:** Interfaces in Shared, implementations in Shared/Repositories

---

## Additional Documentation

| Documentation | Purpose |
|---------------|-----------|
| docs/architectural_patterns.md | Dependency injection, authorization patterns, data access conventions |
| docs/dbcontext-architecture.md | Separate DbContexts strategy, migration patterns |
| docs/permission-system-design-future.md | Future implementation: Page-based authorization, claims caching, Admin bypass logic |
| docs/code-samples/ | Reference implementations for Identity, OpenIddict, and common patterns |

---

## Quick Reference

- **Authentication** - docs/code-samples/openiddict/
- **Authorization** - Current: Role-based via `[Authorize(Roles = "Admin")]`; Future: docs/permission-system-design-future.md:59
- **DbContexts** - docs/dbcontext-architecture.md:7
- **DI Patterns** - docs/architectural_patterns.md:1
- **Input Handling** - docs/architectural_patterns.md:144

---

## Implementation Notes

- Main and IdentityServer reference Shared (no circular dependency)
- IdentityServer migrations use `AuthDbContext` (auth + OpenIddict tables)
- Main app migrations use `VibeCodeDbContext` (business entities)
- OAuth 2.0 with PKCE flow implemented (auto-approve without consent screen)
- Front-channel signout coordinates logout across IdentityServer and Main app
- Role-based authorization available (use `[Authorize(Roles = "Admin")]`)
- **Single Type Per File:** Each file should contain only one class, interface, or enum. Do not place multiple types in a single file.
