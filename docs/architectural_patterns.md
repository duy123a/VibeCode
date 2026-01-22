# Architectural Patterns & Design Decisions

## Dependency Injection

### Registration Patterns

**Scoped Services** (per HTTP request):
- Repositories: `AddScoped<IRepository<T>, Repository<T>>`
- Services: `AddScoped<IUserService, UserService>()`
- DbContext: `AddScoped<AuthDbContext>()`

**Singleton Services** (stateless, application-wide):
- Configuration services (IOptions)
- Cache services (if implemented)

**Transient Services** (lightweight, stateless):
- Validation attributes
- Custom model binders

**Pattern:** Prefer constructor injection over service locator pattern.


---

## Authorization Patterns

### Role-Based Authorization

Current implementation uses ASP.NET Core built-in role-based authorization:

**Usage:**
```csharp
[Authorize(Roles = "Admin")]
public IActionResult AdminPage()
{
    // Only users with Admin role can access
}

[Authorize]
public IActionResult SecurePage()
{
    // Any authenticated user can access
}
```

**Note:** Page-based permission system with claims caching is planned for future implementation. See `docs/permission-system-design-future.md` for design details.

---

## Data Access Patterns

### Repository Pattern

**Interface Location:** `Shared/Interfaces`
**Implementation Location:** `Shared/Repositories`

**Standard Methods:**
- `GetAsync<T>(id)`
- `GetAllAsync<T>()`
- `AddAsync<T>(entity)`
- `UpdateAsync<T>(entity)`
- `DeleteAsync<T>(entity)`

**Unit of Work:**
- Wraps repository operations in transaction
- `CommitAsync()` saves all changes atomically
- Scoped per HTTP request


---

## DbContext Architecture

### Separate DbContexts Strategy

**AuthDbContext (IdentityServer):**
- Purpose: Authentication and authorization
- Entities: AppUser, AppRole, OpenIddict tables
- Migrations: Created in IdentityServer project

**VibeCodeDbContext (Main):**
- Purpose: Business data management
- Entities: Page, Permission, business entities
- Migrations: Created in Main project

**Rationale:**
- Clear separation of concerns
- Independent migration strategies
- Microservices-ready architecture
- Flexible scaling and security

Reference: docs/dbcontext-architecture.md:10

---

## Authentication Patterns

### Two-Login Approach

**Option 1: Simple Login** (ASP.NET Core Identity only)
- Direct authentication on IdentityServer
- No OAuth flow
- Use for: internal systems, quick setup

**Option 2: OAuth 2.0 with PKCE** (OpenIddict)
- IdentityServer redirects to Main app with tokens
- Full authorization code flow
- Use for: production, multiple clients, standard OAuth

**Implementation:** docs/code-samples/openiddict/login-options/

Reference: docs/code-samples/openiddict/login-options/simple-login.cs:1

---

## State Management

### Claims-Based Authentication

**Pattern:** User claims stored in ID tokens from OpenIddict

**Current Claims:**
- Subject (user ID)
- Name
- Email
- Roles
- Display name
- Profile image
- Preferred language

**Note:** Page permission claims system is planned for future implementation. See `docs/permission-system-design-future.md` for design details.

---

## Input Handling Patterns

### Custom Model Binder for String Trimming

**Purpose:** Auto-trim all string inputs to prevent whitespace issues

**Implementation:**
- `TrimStringModelBinder`: Trims string values: docs/code-samples/common/trim-string-model-binder.cs:1
- `TrimmingModelBinderProvider`: Registers binder for string types: docs/code-samples/common/trimming-model-binder-provider.cs:1
- `AddStringTrimModelBinderProvider()` extension method: docs/code-samples/common/extension-method-sample.cs:1

**Configuration:**
```csharp
builder.Services.AddControllersWithViews(o => o.AddStringTrimModelBinderProvider())
```


---

## OpenIddict Integration Patterns

### Server Configuration

**Authorization Code Flow with PKCE:**
- `opt.AllowAuthorizationCodeFlow()`
- `opt.RequireProofKeyForCodeExchange()` (PKCE)
- Endpoint passthrough for custom handling

**Scopes:**
- openid, profile, email, roles
- Registered via `opt.RegisterScopes()`

**Token Storage:**
- Database using OpenIddict entities
- Entities defined in AuthDbContext

Reference: docs/code-samples/openiddict/openiddict-server-config.cs:1

### Client Configuration

**Main App Setup:**
- AddOpenIdConnect() with "OpenIddict" scheme
- PKCE enabled: `options.UsePkce = true`
- Claims in ID token: `options.GetClaimsFromUserInfoEndpoint = false`

**Front-Channel Signout:**
- IdentityServer triggers signout on all clients
- Uses image loading technique

Reference: docs/code-samples/openiddict/login-options/oauth-authorization-flow.cs:1

---

## Validation Patterns

### Data Annotations with Localization

**Pattern:** Use resource files for localized error messages

**ViewModel Example:** docs/code-samples/monolith-identity/viewmodel-sample.cs:1
```csharp
[Display(ResourceType = typeof(SharedResources), Name = "Email")]
[Required(ErrorMessageResourceName = "Required",
        ErrorMessageResourceType = typeof(SharedResources))]
```

**Controller Validation:**
- `ModelState.IsValid` check
- Return View with errors or BadRequest


---

## Error Handling Patterns

### Global Exception Handling

**Pattern:** `app.UseExceptionHandler()` middleware

**Approach:**
- Catch all exceptions globally
- Return `Problem()` with details
- Log with localized messages

**Domain Exceptions:**
- Throw `NotFoundException` for missing resources
- Map to appropriate HTTP status codes


---

## Background Task Patterns

### Fire-and-Forget with DI Scope

**Pattern:** Long-running tasks in background without blocking

**Implementation:**
```csharp
_ = Task.Run(async () =>
{
    using var scope = _serviceProvider.CreateScope();
    var service = scope.ServiceProvider.GetRequiredService<IService>();
    try
    {
        await service.ExecuteAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Background task failed");
    }
});
```

**Key Points:**
- Create new DI scope for background tasks
- Never block main thread
- Handle and log exceptions


---

## Audit & Soft Delete Patterns

### ISoftDeletable Interface

**Purpose:** Soft delete instead of hard delete

**Implementation:**
```csharp
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}
```

**Global Filter:** docs/code-samples/openiddict/auth-dbcontext.cs:21
- Applies `WHERE IsDeleted = false` to all queries
- Manual override: `IgnoreQueryFilters()`

### IAuditable Interface

**Purpose:** Track created/updated metadata

**Implementation:**
```csharp
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }
    string CreatedBy { get; set; }
    DateTimeOffset UpdatedAt { get; set; }
    string UpdatedBy { get; set; }
}
```

**Automatic Population:** docs/code-samples/openiddict/auth-dbcontext.cs:54
- Applied in `SaveChanges()` and `SaveChangesAsync()`
- Uses current user from `IHttpContextAccessor`

---

## CORS Patterns

### CORS Configuration

**Pattern:** Restrict cross-origin requests in production

**Development:**
```csharp
app.UseCors(policy => policy
    .WithOrigins("http://localhost:5001")
    .AllowCredentials());
```

**Production:**
- Restrict to known client origins
- Limit HTTP methods and headers
- Disable credentials if not needed


---

## Async Patterns

### Async/Await Best Practices

**Rules:**
- Use `async/await` for all I/O operations
- Never `async void` (use `async Task`)
- `ConfigureAwait(false)` in libraries/services
- Avoid `.Result` and `.Wait()`

**Repository Pattern:**
- All methods are async: `GetAsync`, `AddAsync`, etc.
- Return `Task<T>` or `Task`

