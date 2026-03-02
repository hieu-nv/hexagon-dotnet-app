# Code Review: hexagon-dotnet-app

**Date:** March 2, 2026

## Overview

A well-structured .NET 10 application following **Hexagonal Architecture (Ports & Adapters)** with Aspire orchestration, Serilog structured logging, FluentValidation, and Datadog integration.

| Project | Role |
|---|---|
| `App.Core` | Domain entities, services, repository interfaces (inner hexagon) |
| `App.Data` | EF Core SQLite implementation (data adapter) |
| `App.Gateway` | External HTTP integrations — PokeAPI adapter |
| `App.Api` | ASP.NET Minimal API (primary adapter) |
| `App.AppHost` | Aspire orchestration |
| `App.ServiceDefaults` | Cross-cutting Aspire defaults |

---

## ✅ Strengths

| Area | Details |
|---|---|
| **Architecture** | Clean hexagonal layering — `App.Core` has zero infrastructure dependencies. Extension method pattern (`UseAppCore()`, `UseAppData()`, `UseAppGateway()`) for module registration |
| **Code Quality** | `ConfigureAwait(false)` throughout, modern primary constructors, XML doc comments, proper guard clauses with `ArgumentNullException.ThrowIfNull()` |
| **Security** | Security headers middleware, rate limiting (100 req/min), FluentValidation filter, RFC 7807 `ProblemDetails` via `GlobalExceptionHandler` |
| **Testing** | Integration tests with `WebApplicationFactory`, per-layer test projects, security header assertions, CRUD lifecycle tests |
| **DevOps** | Multi-stage Dockerfile, non-root user, healthcheck, `.editorconfig`, extensive static analyzers (SonarAnalyzer, SecurityCodeScan, Threading analyzers, BannedApi analyzers) |
| **Observability** | Serilog with JSON console + file + Datadog sinks, OpenTelemetry trace enrichment, correlation IDs in exception handler |

---

## 🔴 High Priority

### 1. `PokeClient` — swallows all exceptions with `Console.Error`

**File:** `src/App.Gateway/Client/PokeClient.cs` (L34-39)

```csharp
catch (Exception ex)
{
    Console.Error.WriteLine($"Error fetching data from {url}: {ex.Message}");
    return null;
}
```

- `Console.Error` bypasses Serilog/OpenTelemetry — **these errors are invisible** in Datadog dashboards
- Catching `OperationCanceledException` hides cancellation
- **Fix:** Inject `ILogger<PokeClient>`, catch only `HttpRequestException`, let cancellation propagate

### 2. CORS — `AllowAnyOrigin()` in production

**File:** `src/App.Api/Program.cs` (L89-100)

`AllowAnyOrigin()` + `AllowAnyMethod()` + `AllowAnyHeader()` is appropriate for a demo but a security risk in production. Configure allowed origins from `appsettings.json`.

### 3. `JsonSerializerOptions` allocated per request

**File:** `src/App.Gateway/Client/PokeClient.cs` (L29-32)

```csharp
new JsonSerializerOptions { PropertyNameCaseInsensitive = true }  // new instance per call
```

Creates GC pressure and foregoes the serializer metadata cache. Use a `static readonly` instance.

---

## 🟡 Medium Priority

### 4. `AppData.UseAppData(WebApplication)` — deferred null guard

**File:** `src/App.Data/AppData.cs` (L54-61)

```csharp
using (var scope = app?.Services.CreateScope())  // null-conditional
{
    var context = scope?.ServiceProvider.GetRequiredService<AppDbContext>();
    context?.Database.EnsureCreated();
}
return app ?? throw new ArgumentNullException(nameof(app));  // guard at the end
```

If `app` is null, the `using` block does nothing, then throws — but the intent isn't clear. Move `ArgumentNullException.ThrowIfNull(app)` to the top.

### 5. Duplicate validation logic

`TodoService.CreateAsync`/`UpdateAsync` check `string.IsNullOrWhiteSpace(entity.Title)`, while `CreateTodoRequestValidator`/`UpdateTodoRequestValidator` enforce `NotEmpty()` + `MaximumLength(200)`. These could diverge as rules evolve.

> **Recommendation:** Keep business invariants in the service layer and input format validation in FluentValidation. Remove the overlapping `IsNullOrWhiteSpace` check from the service, or document why both are needed.

### 6. `PokemonService` — silently clamping parameters

**File:** `src/App.Core/Pokemon/PokemonService.cs` (L29-45)

```csharp
if (limit <= 0) { limit = 20; }
if (limit > 100) { limit = 100; }
if (offset < 0) { offset = 0; }
```

Silently mutating inputs is surprising — callers don't know their value changed. Prefer `Math.Clamp(limit, 1, 100)` with logging already present, or throw `ArgumentOutOfRangeException`.

### 7. Inconsistent null guard style across Endpoints

| Class | Style |
|---|---|
| `PokemonEndpoints` | `?? throw new ArgumentNullException(...)` on constructor params |
| `TodoEndpoints` | No null checks — relies on primary constructor assignment |

Pick one pattern and apply consistently.

### 8. Seed data has past due dates

**File:** `src/App.Data/AppDbContext.cs` (L72-93)

```csharp
DueBy = new DateOnly(2026, 2, 15),  // already past
```

`CreateTodoRequestValidator` enforces `DueBy >= today`, so seed data violates business rules. Use future dates or omit `DueBy` from seed data.

### 9. `TodoRepository.UpdateAsync` — `EntityState.Modified` on untracked entity

**File:** `src/App.Data/Todo/TodoRepository.cs` (L101-113)

`TodoService.UpdateAsync` fetches with `AsNoTracking`, then passes the mutated entity to `UpdateAsync` which sets `EntityState.Modified`. This works but writes **all columns** and will throw if another entity with the same key is tracked.

> **Note:** The `TodoService.UpdateAsync` correctly fetches → mutates → saves, so this works in practice. But it complicates EF Core scenarios where multiple operations share a context scope.

---

## 🟢 Low Priority / Suggestions

### 10. Dead code: `TodoEndpoints.GetDebuggerDisplay()`

**File:** `src/App.Api/Todo/TodoEndpoints.cs` (L193-196)

No `[DebuggerDisplay]` attribute references this method. Remove it.

### 11. `TodoRepository` — unsealed with `virtual` method

`TodoRepository` has `virtual UpdateAsync` and `protected DbContext`, suggesting inheritance, but it's never subclassed. Either `seal` it or make the design intent explicit.

### 12. Security header middleware as inline lambda

The security headers in `src/App.Api/Program.cs` (L126-134) would be cleaner and more testable as a named middleware class in the existing `Middleware/` folder.

### 13. `PokemonGateway` — hardcoded URL in `FetchPokemonByIdAsync`

**File:** `src/App.Gateway/Pokemon/PokemonGateway.cs` (L59)

```csharp
Url = $"https://pokeapi.co/api/v2/pokemon/{id}/",  // hardcoded base URL
```

The `PokeClient` already has a configured `HttpClient` base address. Constructing a URL independently here is fragile — if the base URL changes, this won't update.

### 14. `Entity<T>.Id` is nullable and generic

`Id` is `T?` — for value types like `int`, this means `int?`, which could lead to entities with null IDs being persisted without the database catching it. Consider using a non-nullable `T` with `DatabaseGeneratedOption.Identity`.

### 15. Dockerfile — `AppHost` not excluded from restore

The Dockerfile copies only API/Core/Data/Gateway/ServiceDefaults projects but restores the full `App.slnx` which references `AppHost`. This may cause restore warnings or failures if AppHost has dependencies not present in the container.

---

## Summary Scorecard

| Category | Score | Notes |
|---|---|---|
| Architecture | ⭐⭐⭐⭐⭐ | Excellent hexagonal layering, clean DI |
| Code Quality | ⭐⭐⭐⭐ | Minor inconsistencies, dead code |
| Security | ⭐⭐⭐⭐ | Good headers & rate limiting; CORS needs tightening |
| Performance | ⭐⭐⭐⭐ | `AsNoTracking` used well; JSON options & caching well-configured |
| Testing | ⭐⭐⭐⭐ | Solid per-layer coverage; could add more negative/edge cases |
| Error Handling | ⭐⭐⭐ | `PokeClient` is the main gap; `GlobalExceptionHandler` is good |
| DevOps | ⭐⭐⭐⭐½ | Multi-stage Docker, analyzers, Aspire; minor Dockerfile issue |
| Observability | ⭐⭐⭐⭐ | Serilog + OpenTelemetry + Datadog; `Console.Error` in PokeClient undermines it |

**Overall: Strong codebase.** The architecture is clean, security posture is solid for a demo/starter project, and testing is well-structured. The top-priority fix remains the `PokeClient` error handling — it undermines the otherwise excellent observability setup.
