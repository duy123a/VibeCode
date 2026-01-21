# AGENTS.md

This file contains guidelines for agentic coding agents working in the VibeCode repository.

## Project Structure
- Solution: VibeCode.IdentityServer (OpenIddict) + VibeCode.Main (MVC) + VibeCode.Shared
- VibeCode.Shared contains shared entities, interfaces, DTOs, validation, resources
- Main and IdentityServer both reference Shared
- Architecture: Clean Architecture with layered approach
- Use existing patterns, avoid introducing new ones unless necessary
- Infrastructure folder in Shared/Repositories for EF Core repository implementations

## Build Commands
- `dotnet clean && dotnet build` - Clean and build entire solution
- `dotnet clean <Project> && dotnet build <Project>` - Clean/build specific project
- `dotnet clean && dotnet build --configuration Release` - Release build
- `dotnet run --project VibeCode.IdentityServer` - Run IdentityServer
- `dotnet run --project VibeCode.Main` - Run Main app

## Testing Commands
- `dotnet test` - Run all tests (NUnit + Moq)
- `dotnet test --filter "FullyQualifiedName~ClassName"` - Run single test class
- `dotnet test --filter "FullyQualifiedName~ClassName.MethodName"` - Run single test method
- Prefer unit tests over integration tests
- Use Moq for mocking dependencies in unit tests
- Focus on testing business logic in services, not controllers

## Database Commands
- SQL Server as database provider
- `dotnet ef migrations add <Name> --project <WebProject>` - Create migration
- `dotnet ef migrations script` - Generate SQL script
- `dotnet ef migrations add <Name> --startup-project <WebProject>` - Create from web project
- **CRITICAL: Do NOT apply migrations (dotnet ef database update) unless explicitly requested**
- SQL scripts must be reviewed before execution
- Migrations stored in IdentityServer project (AuthDbContext) and Main project (VibeCodeDbContext)

## C# Code Style
- .NET 9.0 target framework
- Nullable reference types: enabled
- Implicit usings: enabled
- PascalCase for public members, camelCase for private fields/params
- _underscore prefix for private fields, use `var` when obvious
- `this.` qualifier allowed

## Data Access Patterns
- Repository pattern: interfaces in Shared/Interfaces, impls in Infrastructure
- UnitOfWork for transaction management
- Methods: GetAsync, GetAllAsync, AddAsync, UpdateAsync, DeleteAsync
- UnitOfWork.CommitAsync() for saving changes
- DbContext scoped per HTTP request
- IQueryable for complex queries, IEnumerable for materialized
- SQL Server as primary database

## MVC Conventions
- Controllers inherit from `Controller`, actions return IActionResult
- Views: Views/{Controller}/{Action}.cshtml
- Models as DTOs/ViewModels in Models folder
- Dependency injection via constructor
- Validate using ModelState and Data Annotations
- Use [HttpGet], [HttpPost] attributes

## Model Validation
- Data Annotations: [Required], [StringLength], [EmailAddress], [Range]
- ModelState.IsValid in controllers for validation checks
- Return BadRequest(ModelState) or View with errors
- Create custom validation attributes in Shared/Validation
- Display names/messages from shared resource files

## Input Handling
- TrimmingModelBinder auto-trims all string inputs
- Configure: builder.Services.AddControllersWithViews(o => o.AddStringTrimModelBinderProvider())
- All string inputs trimmed before binding
- Prevents leading/trailing whitespace issues

## Localization & Resources
- Single shared resource: SharedResource.resx, SharedResource.en.resx, SharedResource.vn.resx
- Place in Shared/Resources or Properties/SharedResource
- IStringLocalizer<SharedResource> in controllers, IHtmlLocalizer<SharedResource> in views
- Usage: _localizer["Key"], @localizer["Key"]
- Configure in Program.cs: AddLocalization(), UseRequestLocalization()

## Shared Project
- VibeCode.Shared: entities, DTOs, interfaces, helpers, validation, resources
- Shared/Entities: domain models (POCOs) including AppUser, AppRole, Page, Permission, ISoftDeletable, IAuditable
- Shared/Interfaces: IRepository, IService, IUnitOfWork
- Shared/Repositories: Repository implementations for EF Core
- Shared/Models: DTOs, ViewModels
- Shared/Validation: custom validation attributes
- Shared/Resources: resource files for multi-language

## OpenIddict Integration
- AddOpenIddict() in Program.cs
- Use AuthorizationCodeFlow for OAuth 2.0 / OpenID Connect
- Enable PKCE (RequireProofKeyForCodeExchange())
- Configure authorization, token, logout endpoints
- AddAuthorization() for policies
- AddAuthentication() with OpenIddict scheme
- Register OpenIddict client for Main app
- [Authorize] on protected controllers
- Store tokens in database using OpenIddict entities

## Page-Based Authorization
- Pages stored in database (Page entity: Id, Name, Route, Controller, Action)
- Permissions saved per page (Permission entity: Id, UserId/RoleId, PageId, CanAccess)
- Authorization checks user permissions against requested page route
- Admin role bypasses permission checks automatically
- Policy-based authorization: [Authorize(Policy = "PageAccess")]
- Implement IAuthorizationHandler for page permission checks
- Page permissions loaded and cached on user login as claims
- Use user claims for page access: User.HasClaim("page_permission", "PageName")
- Authorize attribute on controllers/actions: [Authorize(Policy = "PageAccess")]

## CORS Configuration
- AddCors() in Program.cs
- Define allowed origins for Main app and other clients
- app.UseCors() before UseAuthorization in middleware
- WithOrigins("http://localhost:5001").AllowCredentials()
- Allow specific methods, headers as needed
- Restrict CORS in production

## Naming Conventions
- Controllers: *Controller, Views: *.cshtml matching actions
- Models: *ViewModel, *Model, *Dto
- Interfaces: I*, Repositories: I*Repository, *Repository
- Services: I*Service, *Service
- Async methods: *Async (GetUserAsync)
- Resource classes: SharedResource

## Error Handling & Logging
- Global exception: app.UseExceptionHandler()
- Return Problem() or BadRequest() with details
- ILogger<T> for logging, Activity.Current?.Id for correlation
- Throw domain exceptions: NotFoundException
- Log with resource-localized messages

## Dependency Injection
- Register services in Program.cs with builder.Services
- Scoped for services/repositories, Singleton for stateless
- Transient for lightweight services
- Prefer constructor injection

## Security
- Validate via ModelState, [ValidateAntiForgeryToken] on POST
- HTTPS: app.UseHttpsRedirection()
- Never log sensitive data
- Secure cookies, proper AuthN/AuthZ
- Page-based authorization for access control

## Async Patterns
- async/await for all I/O operations
- Never async void (use async Task)
- ConfigureAwait(false) in libraries/services
- Avoid .Result/.Wait()

## Background Tasks
- Long-running tasks (email sending, file processing) run in background
- Create new DI scope: using var scope = _serviceProvider.CreateScope()
- Execute in background: Task.Run(async () => { ... })
- Resolve services from new scope: scope.ServiceProvider.GetRequiredService<T>()
- Do NOT block main thread - use Task.Run or fire-and-forget
- Handle exceptions with try/catch and log errors
- Use IHostedService/BackgroundService for recurring tasks
- Example: _ = Task.Run(async () => { using var scope = ...; var mailer = scope.ServiceProvider...; await mailer.SendAsync(); })

## Entity Framework
- EF Core with SQL Server
- IdentityServer: AuthDbContext (AppUser, AppRole, OpenIddict entities)
- Main: VibeCodeDbContext (business entities)
- Repositories in Shared/Repositories (interfaces + implementations)
- dotnet ef migrations add <Name>
- Include() for eager loading, disable lazy loading
- AsNoTracking() for read-only queries
- UnitOfWork for transactions

## Service Layer
- Services handle business logic, interfaces for testability
- Injected into controllers, work with repos via UnitOfWork
- Async throughout, return DTOs not entities
- Services in Application or Services folder

## Unit Testing Guidelines
- Use NUnit for test framework
- Use Moq for mocking dependencies (repositories, services)
- Prefer unit tests over integration tests
- Test service layer logic, not controller logic
- Mock all external dependencies (DB, HTTP, File I/O)
- Test happy path and edge cases
- Use [Test], [TestCase] attributes for tests
- Assert with NUnit assertions: Assert.AreEqual, Assert.IsTrue
- Arrange-Act-Assert pattern for test structure

## Custom Data Annotations
- Custom attributes in Shared/Validation
- Inherit ValidationAttribute or implement IModelValidator
- Use IStringLocalizer for error messages
- Examples: [CustomEmail], [StrongPassword], [UniqueUsername]
- Register for reuse

## Code Organization
- Keep controllers thin - delegate to services
- Business logic in services, data access in repositories
- Entities as POCOs in Shared, add behavior to entities
- Avoid anemic domain models
- Keep views simple - use helpers/partials
- Main references Shared, IdentityServer references Shared
