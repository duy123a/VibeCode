# DbContext Architecture Strategy

## Overview

This project uses two separate DbContexts for different purposes, following a clean separation of concerns.

## DbContexts

### 1. AuthDbContext (IdentityServer)

**Location:** VibeCode.IdentityServer

**Purpose:** Authentication and authorization

**Entities:**
- ASP.NET Core Identity tables (AspNetUsers, AspNetRoles, AspNetUserClaims, etc.)
- OpenIddict tables (OpenIddictApplications, OpenIddictAuthorizations, OpenIddictTokens, OpenIddictScopes)

**Usage:**
- User authentication (login, logout)
- Token management (OAuth/OIDC tokens)
- Application and scope management
- Authorization code flow

**Migrations:** Created and run from IdentityServer project

---

### 2. VibeCodeDbContext (Main)

**Location:** VibeCode.Main

**Purpose:** Business data management

**Entities:**
- Page (pages for authorization system)
- Permission (user/role permissions for pages)
- Custom business entities

**Usage:**
- Page management
- Permission management
- Business logic data operations
- Permission authorization checks (if needed)

**Migrations:** Created and run from Main project

---

## Decision Rationale

### Why Separate DbContexts?

1. **Clear Separation of Concerns**
   - AuthDbContext focuses solely on auth/authz
   - VibeCodeDbContext focuses on business logic
   - Easy to understand and maintain

2. **Database Flexibility**
   - Can use separate databases if needed
   - AuthDb on secure server, BusinessDb on performance-optimized server
   - Migration strategies can be independent

3. **Microservices Ready**
   - When splitting into microservices, IdentityServer can have its own database
   - Main app can have its own business database
   - No coupling between auth and business data

4. **Performance Optimization**
   - Can optimize each database independently
   - AuthDb can use different indexing strategy
   - BusinessDb can scale independently

5. **Security**
   - Auth database with stricter access controls
   - Business database with different security policies
   - Limited blast radius if one database is compromised

---

## Shared Entities Pattern

### Entities in Shared Project

Both DbContexts reference entities from `VibeCode.Shared`:

**Authentication Entities (Shared):**
- `AppUser` : IdentityUser<string>
- `AppRole` : IdentityRole<string>
- `ISoftDeletable` - Interface for soft delete pattern
- `IAuditable` - Interface for audit tracking

**Authorization Entities (Shared):**
- `Page` - Page definition for authorization
- `Permission` - User/role permissions for pages

### How It Works

```
VibeCode.Shared (Entities)
    ↓
    ├─→ AuthDbContext (IdentityServer)
    │     - AppUser, AppRole tables
    │     - OpenIddict tables
    │
    └─→ VibeCodeDbContext (Main)
          - Page, Permission tables
          - Business entities
```

Both DbContexts define `DbSet<T>` for shared entities.

---

## Migration Strategy

### IdentityServer Migrations

```bash
# From solution root
dotnet ef migrations add AddIdentitySetup --project VibeCode.IdentityServer --startup-project VibeCode.IdentityServer

# Generate SQL script
dotnet ef migrations script --project VibeCode.IdentityServer --startup-project VibeCode.IdentityServer
```

### Main App Migrations

```bash
# From solution root
dotnet ef migrations add AddPageSetup --project VibeCode.Main --startup-project VibeCode.Main

# Generate SQL script
dotnet ef migrations script --project VibeCode.Main --startup-project VibeCode.Main
```

### Important Notes

- Migrations are project-specific (one set for IdentityServer, one for Main)
- Shared entities can be included in both DbContexts
- Run migrations separately for each project
- Never run `dotnet ef database update` without explicit approval

---

## Future Microservices Migration

When migrating to microservices architecture:

### Phase 1: Separate Databases (Current Approach)
- IdentityServer uses AuthDb
- Main app uses BusinessDb
- Both reference Shared entities
- API calls between services for cross-domain data

### Phase 2: Full Microservices
- IdentityServer as standalone auth service
- Main app as business service
- Shared entities moved to separate NuGet package
- Communication via gRPC/REST APIs
- Each service has its own database

---

## Alternative: Single DbContext (Rejected)

### What Would It Look Like?

One DbContext containing all tables (auth + business).

### Why Rejected?

1. **Tight Coupling:** Auth and business logic tightly coupled
2. **Migration Complexity:** Single migration chain for all features
3. **Scaling Issues:** Cannot scale auth and business independently
4. **Migration Difficulties:** Harder to split into microservices later
5. **Clearer Separation:** Mixed concerns in same DbContext

### When Might Single DbContext Be OK?

- Very small projects (proof of concept)
- Single developer project
- No plans for microservices
- Limited scalability requirements

---

## Code Examples

### AuthDbContext (IdentityServer)

```csharp
public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // OpenIddict entities
    public DbSet<OpenIddictEntityFrameworkCoreApplication> Applications { get; set; }
    public DbSet<OpenIddictEntityFrameworkCoreAuthorization> Authorizations { get; set; }
    public DbSet<OpenIddictEntityFrameworkCoreScope> Scopes { get; set; }
    public DbSet<OpenIddictEntityFrameworkCoreToken> Tokens { get; set; }

    // AppUser and AppRole are added via AddIdentity<AppUser, AppRole>()
    // No need to declare them as DbSets

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.UseOpenIddict();

        // Global query filter for soft delete
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AuthDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { builder });
            }
        }
    }
}
```

### VibeCodeDbContext (Main)

```csharp
public class VibeCodeDbContext : DbContext
{
    public VibeCodeDbContext(DbContextOptions<VibeCodeDbContext> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Business entities
    public DbSet<Page> Pages { get; set; }
    public DbSet<Permission> Permissions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Global query filter for soft delete
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(VibeCodeDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { builder });
            }
        }
    }

    // Include ApplyAuditInfo and SaveChanges overrides
}
```

---

## Summary

- **Separate DbContexts** for auth and business logic
- **Shared entities** in VibeCode.Shared referenced by both
- **Clear separation** of concerns and responsibilities
- **Microservices-ready** architecture
- **Independent migrations** for each project
- **Flexible scaling** and security options
