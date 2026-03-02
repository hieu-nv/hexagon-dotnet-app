# Fix Roadmap for Code Review Findings

**Date:** March 2, 2026  
**Related:** [CODE_REVIEW_2026_03_02.md](./CODE_REVIEW_2026_03_02.md)

Phased plan to address all 15 findings. Each phase is independently shippable.

---

## Phase 1 — Critical Fixes (High Priority)

Quick wins that address security and observability gaps.

### 1.1 `PokeClient` Error Handling (Finding #1, #3)

**File:** `src/App.Gateway/Client/PokeClient.cs`

| Change | Details |
|---|---|
| Inject `ILogger<PokeClient>` | Via primary constructor |
| Replace `Console.Error.WriteLine` | Use `_logger.LogError` |
| Narrow catch clause | Catch `HttpRequestException` only; let `OperationCanceledException` propagate |
| Static `JsonSerializerOptions` | Extract to `private static readonly` field |

### 1.2 CORS Policy (Finding #2)

**Files:** `src/App.Api/Program.cs`, `src/App.Api/appsettings.json`

| Change | Details |
|---|---|
| Config-driven origins | Read `Cors:AllowedOrigins` from configuration |
| Dev fallback | Fall back to `AllowAnyOrigin()` only when `IsDevelopment()` |

---

## Phase 2 — Code Consistency & Correctness (Medium Priority)

### 2.1 Null Guard in `AppData` (Finding #4)

**File:** `src/App.Data/AppData.cs`

- Move `ArgumentNullException.ThrowIfNull(app)` to top of `UseAppData(WebApplication)`
- Remove null-conditional operators inside the method

### 2.2 Remove Duplicate Validation (Finding #5)

**File:** `src/App.Core/Todo/TodoService.cs`

- Remove `string.IsNullOrWhiteSpace(entity.Title)` checks from `CreateAsync`/`UpdateAsync`
- Keep `ArgumentNullException.ThrowIfNull(entity)` (code contract, not validation)

### 2.3 Explicit Parameter Clamping (Finding #6)

**File:** `src/App.Core/Poke/PokemonService.cs`

- Replace 3 if-blocks → `limit = Math.Clamp(limit, 1, 100)` + `offset = Math.Max(0, offset)`
- Keep existing warning logs

### 2.4 Consistent Null Guards (Finding #7)

**File:** `src/App.Api/Todo/TodoEndpoints.cs`

- Add `?? throw new ArgumentNullException(...)` to constructor field assignments (match `PokemonEndpoints`)

### 2.5 Fix Seed Data Dates (Finding #8)

**File:** `src/App.Data/AppDbContext.cs`

- Remove `DueBy` from seed data entries (seed data is static; can't use relative dates)

---

## Phase 3 — Refinements (Low Priority)

### 3.1 Remove Dead Code (Finding #10)

**File:** `src/App.Api/Todo/TodoEndpoints.cs`

- Delete `GetDebuggerDisplay()` method and `System.Diagnostics` using

### 3.2 Seal `TodoRepository` (Finding #11)

**File:** `src/App.Data/Todo/TodoRepository.cs`

- Add `sealed` to class
- Remove `virtual` from `UpdateAsync`
- Change `protected DbContext` → `private`

### 3.3 Extract Security Headers Middleware (Finding #12)

**New file:** `src/App.Api/Middleware/SecurityHeadersMiddleware.cs`  
**Modify:** `src/App.Api/Program.cs`

- Implement `IMiddleware` with the 5 security headers
- Replace inline lambda with `app.UseMiddleware<SecurityHeadersMiddleware>()`
- Register in DI

### 3.4 Fix Hardcoded URL (Finding #13)

**File:** `src/App.Gateway/Poke/PokemonGateway.cs`

- Use relative URL `$"pokemon/{id}/"` instead of hardcoding PokeAPI base URL

### 3.5 Dockerfile Cleanup (Finding #15)

**File:** `Dockerfile`

- Copy `AppHost` `.csproj` so `dotnet restore App.slnx` succeeds, **or** restore only `App.Api.csproj`

---

## Deferred

| Finding | Reason |
|---|---|
| #9 — EF update pattern (`EntityState.Modified` on untracked entity) | Requires deeper refactoring; works correctly in practice |
| #14 — Nullable `Entity<T>.Id` | Schema change; needs migration planning |

---

## Verification Plan

### After Each Phase

```bash
# Build (with warnings-as-errors)
dotnet build src/App.slnx --warnaserror -c Release

# Run all tests
dotnet test src/App.slnx --verbosity normal
```

### After Phase 3 (Dockerfile)

```bash
docker build -t hexagon-dotnet-app:test .
```

### Test Coverage Map

| Phase | Files Modified | Test Files Covering Changes |
|---|---|---|
| 1.1 | `PokeClient.cs` | `App.Gateway.Tests/PokeClientTests` |
| 1.2 | `Program.cs` | `App.Api.Tests/Integration/*` |
| 2.1 | `AppData.cs` | `App.Data.Tests/*` |
| 2.2 | `TodoService.cs` | `App.Core.Tests/Todo/TodoServiceTests` |
| 2.3 | `PokemonService.cs` | `App.Core.Tests/Poke/PokemonServiceTests` |
| 2.4 | `TodoEndpoints.cs` | `App.Api.Tests/Todo/TodoEndpointsTests` |
| 2.5 | `AppDbContext.cs` | `App.Data.Tests/*` |
| 3.1 | `TodoEndpoints.cs` | `App.Api.Tests/Todo/TodoEndpointsTests` |
| 3.2 | `TodoRepository.cs` | `App.Data.Tests/*` |
| 3.3 | `SecurityHeadersMiddleware.cs` (new), `Program.cs` | `App.Api.Tests/Integration/*` |
| 3.4 | `PokemonGateway.cs` | `App.Gateway.Tests/*` |
| 3.5 | `Dockerfile` | Docker build test |
