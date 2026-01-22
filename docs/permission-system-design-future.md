# Permission System Design

## Decision Summary
Policy-based authorization with claims caching for page-level permissions.

## Architecture

### Components
- **Storage**: `Page` and `Permission` entities in `VibeCode.Shared`
- **Authentication**: IdentityServer (OpenIddict + ASP.NET Core Identity)
- **Authorization**: Main app (policy-based checks using claims)

### Permission Flow
1. User logs in → IdentityServer
2. IdentityServer loads user's page permissions from database
3. Permissions added as claims in ID token (`page_permission` claim)
4. Main app validates claims using policy-based authorization
5. Access granted/denied based on claims

## Implementation Plan

### 1. Entities in Shared (VibeCode.Shared/Entities)

```csharp
public class Page : ISoftDeletable, IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}

public class Permission : ISoftDeletable, IAuditable
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? RoleId { get; set; }
    public int PageId { get; set; }
    public bool CanAccess { get; set; }
}
```

### 2. Claims Generation in IdentityServer

On successful login (in `AccountController` or custom service):

```csharp
var userPermissions = await _unitOfWork.Permissions.GetAllAsync(p =>
    (p.UserId == user.Id || _userManager.IsInRoleAsync(user, p.RoleId).Result)
    && p.CanAccess);

var pageClaims = userPermissions.Select(p =>
    new Claim("page_permission", p.Page.Name));

await _userManager.AddClaimsAsync(user, pageClaims);
```

### 3. Authorization in Main App

**Requirement class:**
```csharp
public class PageAccessRequirement : IAuthorizationRequirement { }
```

**Handler:**
```csharp
public class PageAuthorizationHandler : AuthorizationHandler<PageAccessRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PageAccessRequirement requirement)
    {
        // Admin role bypasses permission checks
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Get current page route from route data
        // Check if user has page_permission claim for this route
        // Set requirement satisfied if claim exists
    }
}
```

**Policy registration (Program.cs):**
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PageAccess", policy =>
        policy.Requirements.Add(new PageAccessRequirement()));
});
builder.Services.AddScoped<IAuthorizationHandler, PageAuthorizationHandler>();
```

**Controller usage:**
```csharp
[Authorize(Policy = "PageAccess")]
public IActionResult AdminDashboard()
{
    return View();
}
```

### 4. Permission Management UI

- CRUD pages for `Page` entities
- CRUD pages for `Permission` entities
- Assign permissions to users/roles per page

## Advantages

1. **Performance**: No database queries per request (claims-based)
2. **Separation**: IdentityServer handles auth, Main app handles authz
3. **Flexibility**: Easy to add complex authorization logic in handlers
4. **Built-in**: Uses ASP.NET Core authorization infrastructure
5. **Testable**: Can mock claims for testing
6. **Admin bypass**: Admin role automatically bypasses permission checks (simplified authorization)

## Alternative Approaches (Rejected)

### Database Lookup Per Request
- ❌ Performance impact
- ❌ Database dependency in request flow

### Custom [PageAuthorize] Attribute
- ❌ More code to maintain
- ❌ Less flexible than policy-based
- ❌ Doesn't leverage built-in authorization

### IdentityServer Authorization Checks
- ❌ Main app would depend on IdentityServer for authz
- ❌ Tightly coupled architecture

## Configuration Notes

- Page permissions loaded on login in IdentityServer
- Claims included in ID token via OpenIddict
- Main app validates claims on each protected request
- Admin role bypasses permission checks automatically
- Session-based (no re-querying database after login)
- Permission changes require user to re-login for new permissions to take effect
