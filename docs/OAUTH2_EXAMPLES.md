# OAuth2 Code Examples

## Use Authenticated User in Endpoint Handlers

```csharp
// Using AuthService directly (injected)
app.MapGet("/my-endpoint", (AuthService authService, ClaimsPrincipal user) =>
{
    var authenticatedUser = authService.GetAuthenticatedUser(user);
    if (authenticatedUser is null) return Results.Unauthorized();

    return Results.Ok($"Hello, {authenticatedUser.Name}!");
}).RequireAuthorization();
```

```csharp
// Using AuthServiceExtensions convenience method
app.MapGet("/my-endpoint", (IServiceProvider sp, ClaimsPrincipal user) =>
{
    var authenticatedUser = sp.GetAuthenticatedUser(user);
    if (authenticatedUser is null) return Results.Unauthorized();

    return Results.Ok(new { authenticatedUser.Email, authenticatedUser.Roles });
}).RequireAuthorization();
```

## Define and Use Authorization Policies

```csharp
// Apply a named policy in endpoint mapping
group.MapPost("/items", handler)
     .RequireAuthorization(AuthorizationPolicies.TodoAccess);

// Apply AdminOnly policy
group.MapGet("/reports", handler)
     .RequireAuthorization(AuthorizationPolicies.AdminOnly);

// Require any authenticated user
group.MapGet("/profile", handler)
     .RequireAuthorization();
```

## Extract Custom Claims

```csharp
var authenticatedUser = authService.GetAuthenticatedUser(user);

// Access custom claims (non-standard OpenID Connect claims)
if (authenticatedUser?.CustomClaims.TryGetValue("department", out var department) == true)
{
    Console.WriteLine($"User department: {department}");
}
```

## Check Roles Programmatically with Helpers

```csharp
using App.Api.Auth;

var authenticatedUser = authService.GetAuthenticatedUser(user);

// Check a single role
bool isAdmin = AuthorizationHelpers.HasRole(authenticatedUser!, "admin");

// OR logic (user has at least one)
bool canEdit = AuthorizationHelpers.HasAnyRole(authenticatedUser!, ["editor", "admin"]);

// AND logic (user must have all)
bool isSuperAdmin = AuthorizationHelpers.HasAllRoles(authenticatedUser!, ["admin", "super"]);
```

## Manual Authorization Check with AuthorizationService

```csharp
// Injected via DI
app.MapGet("/conditional", (
    App.Api.Auth.AuthorizationService authzService,
    AuthService authService,
    ClaimsPrincipal user) =>
{
    var authenticatedUser = authService.GetAuthenticatedUser(user);
    if (authenticatedUser is null) return Results.Unauthorized();

    // Domain-level authorization (in addition to ASP.NET Core policy)
    if (!authzService.AuthorizeUser(authenticatedUser, AuthorizationPolicies.AdminOnly))
        return Results.Forbid();

    return Results.Ok("Admin access granted");
});
```

## Configuration-Driven Auth Toggle

JWT is conditionally enabled via `JwtBearer:Enabled` in `appsettings.json`. When disabled, all endpoints are accessible without authentication:

```json
{
  "JwtBearer": {
    "Enabled": false
  }
}
```

This is useful for local development without Keycloak running.
