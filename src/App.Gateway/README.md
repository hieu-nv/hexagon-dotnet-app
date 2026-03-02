# App.Gateway Module

## Overview

The **App.Gateway** module is a secondary adapter in the hexagonal architecture that implements gateway interfaces for interacting with external services and APIs. This module connects the domain (Core) to external systems while maintaining the separation of concerns principle.

## Purpose

The Gateway module serves as an abstraction layer for:

- External REST API integrations
- Third-party service interactions
- Remote data source access
- Any external system communication

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    App.Api (Primary Adapter)            │
│              HTTP Endpoints expose Pokemon data         │
└────────────────────┬────────────────────────────────────┘
                     │ uses
┌────────────────────▼────────────────────────────────────┐
│                    App.Core (Domain)                     │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Pokemon (Domain Model)                          │   │
│  │  IPokemonGateway (Port/Interface)                │   │
│  └──────────────────────────────────────────────────┘   │
└────────────────────┬────────────────────────────────────┘
                     │ implements
┌────────────────────▼────────────────────────────────────┐
│              App.Gateway (Secondary Adapter)             │
│  ┌──────────────────────────────────────────────────┐   │
│  │  PokemonGateway (implements IPokemonGateway)     │   │
│  │  PokeClient (HTTP Client)                        │   │
│  │  PokeResponse (Response Models)                  │   │
│  └──────────────────────────────────────────────────┘   │
└────────────────────┬────────────────────────────────────┘
                     │ calls
           ┌─────────▼──────────┐
           │  External PokeAPI  │
           │  https://pokeapi.co│
           └────────────────────┘
```

## Project Structure

```
App.Gateway/
├── App.Gateway.csproj          # Project configuration
├── AppGateway.cs               # Extension methods for DI registration
├── Client/                     # HTTP client infrastructure
│   ├── IPokeClient.cs          # HTTP client interface
│   ├── PokeClient.cs           # HTTP client implementation
│   └── PokeResponse.cs         # Generic response wrapper
└── Poke/                       # Pokemon gateway implementation
    └── PokemonGateway.cs       # Implements IPokemonGateway
```

## Key Components

### 1. AppGateway Configuration

The `AppGateway` class provides extension methods for registering gateway services:

```csharp
builder.UseAppGateway();  // Registers HTTP clients and gateway implementations
```

### 2. HTTP Client (PokeClient)

Generic HTTP client for making requests to external APIs:

- Configured with base address and default headers
- Handles JSON deserialization
- Includes error handling and logging
- Uses HttpClientFactory for efficient connection management

### 3. Gateway Implementations

**PokemonGateway**: Implements the `IPokemonGateway` interface defined in Core:

- `FetchPokemonListAsync(limit, offset)` - Fetches paginated Pokemon list
- `FetchPokemonByIdAsync(id)` - Fetches specific Pokemon details

## Usage Example

### 1. Define Gateway Interface in Core

```csharp
// App.Core/Poke/IPokemonGateway.cs
public interface IPokemonGateway
{
    Task<IEnumerable<Pokemon>?> FetchPokemonListAsync(int limit = 20, int offset = 0);
    Task<Pokemon?> FetchPokemonByIdAsync(int id);
}
```

### 2. Implement Gateway in Gateway Module

```csharp
// App.Gateway/Poke/PokemonGateway.cs
public class PokemonGateway(IPokeClient pokeClient) : IPokemonGateway
{
    public async Task<IEnumerable<Pokemon>?> FetchPokemonListAsync(int limit = 20, int offset = 0)
    {
        var url = $"pokemon?limit={limit}&offset={offset}";
        var response = await _pokeClient.GetAsync<PokeResponse<PokemonItem>>(url);
        return response?.Results.Select(item => new Pokemon { ... });
    }
}
```

### 3. Register in Program.cs

```csharp
builder.UseAppGateway();  // Register gateway services
```

### 4. Use in API Endpoints

```csharp
// App.Api/Poke/PokemonEndpoints.cs
internal sealed class PokemonEndpoints(IPokemonGateway pokemonGateway)
{
    public async Task<IResult> FetchPokemonListAsync(int limit = 20, int offset = 0)
    {
        var pokemon = await _pokemonGateway.FetchPokemonListAsync(limit, offset);
        return Results.Ok(pokemon);
    }
}
```

## Testing the Gateway

### HTTP Requests

```http
# Get Pokemon list
GET http://localhost:5112/api/v1/pokemon?limit=20&offset=0

# Get specific Pokemon
GET http://localhost:5112/api/v1/pokemon/25
```

### Expected Response

```json
[
  {
    "name": "bulbasaur",
    "url": "https://pokeapi.co/api/v2/pokemon/1/"
  },
  {
    "name": "ivysaur",
    "url": "https://pokeapi.co/api/v2/pokemon/2/"
  }
]
```

## Benefits of the Gateway Pattern

1. **Separation of Concerns**: External API details are isolated from business logic
2. **Testability**: Easy to mock gateway interfaces for unit testing
3. **Flexibility**: Can swap external services without changing core logic
4. **Resilience**: Centralized error handling and retry logic
5. **Type Safety**: Strong typing for external API responses

## Adding New Gateways

To add a new external service gateway:

1. **Define the interface in Core**:

   ```csharp
   // App.Core/Weather/IWeatherGateway.cs
   public interface IWeatherGateway
   {
       Task<Weather?> GetCurrentWeatherAsync(string city);
   }
   ```

2. **Implement in Gateway module**:

   ```csharp
   // App.Gateway/Weather/WeatherGateway.cs
   public class WeatherGateway(HttpClient httpClient) : IWeatherGateway { }
   ```

3. **Register in AppGateway.cs**:

   ```csharp
   builder.Services.AddHttpClient<IWeatherGateway, WeatherGateway>(/* config */);
   ```

4. **Use in API endpoints**:
   ```csharp
   // App.Api/Weather/WeatherEndpoints.cs
   public class WeatherEndpoints(IWeatherGateway weatherGateway) { }
   ```

## Dependencies

- **App.Core**: References domain models and gateway interfaces
- **Microsoft.Extensions.Http**: Provides HttpClient factory
- **System.Text.Json**: JSON serialization

## Best Practices

1. **Use HttpClientFactory**: Registered via `AddHttpClient<T>()` for connection pooling
2. **Configure timeouts**: Set appropriate timeout values for external APIs
3. **Handle failures gracefully**: Return null or throw specific exceptions
4. **Cache responses**: Consider caching for frequently accessed data
5. **Use async/await**: All gateway methods should be asynchronous
6. **Configure retries**: Implement retry policies for transient failures (e.g., Polly)

## References

- [Hexagonal Architecture - Alistair Cockburn](https://alistair.cockburn.us/hexagonal-architecture/)
- [Understanding Hexagonal Architecture (Medium Article)](https://medium.com/@hieunv/understanding-hexagonal-architecture-through-a-practical-application-2f2d28f604d9)
- [HttpClient Best Practices](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)
