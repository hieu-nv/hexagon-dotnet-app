using System.Net;
using System.Net.Http.Json;
using App.Api.Todo;
using Xunit;

namespace App.Api.Tests.Integration;

public class TodoIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;

    public TodoIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllTodos_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/todos");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateAndGetTodo_ShouldReturnCreatedTodo()
    {
        // Arrange
        var request = new CreateTodoRequest("Integration Test Todo", false, null);

        // Act - Create
        var createResponse = await _client.PostAsJsonAsync("/api/v1/todos", request);

        // Assert - Create
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.NotNull(created);
        Assert.Equal("Integration Test Todo", created.Title);
        Assert.False(created.IsCompleted);

        // Act - Get by ID
        var getResponse = await _client.GetAsync($"/api/v1/todos/{created.Id}");

        // Assert - Get
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var retrieved = await getResponse.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal("Integration Test Todo", retrieved.Title);
    }

    [Fact]
    public async Task CreateUpdateAndGetTodo_ShouldReturnUpdatedTodo()
    {
        // Arrange
        var createRequest = new CreateTodoRequest("Original Title", false, null);

        // Act - Create
        var createResponse = await _client.PostAsJsonAsync("/api/v1/todos", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.NotNull(created);

        // Act - Update
        var updateRequest = new UpdateTodoRequest("Updated Title", true, null);
        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/v1/todos/{created.Id}",
            updateRequest
        );

        // Assert - Update
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Updated Title", updated.Title);
        Assert.True(updated.IsCompleted);
    }

    [Fact]
    public async Task CreateAndDeleteTodo_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest = new CreateTodoRequest("Todo to Delete", false, null);
        var createResponse = await _client.PostAsJsonAsync("/api/v1/todos", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.NotNull(created);

        // Act - Delete
        var deleteResponse = await _client.DeleteAsync($"/api/v1/todos/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify it's gone
        var getResponse = await _client.GetAsync($"/api/v1/todos/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetTodoById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/todos/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTodoById_WithInvalidId_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/todos/0");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCompletedTodos_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/todos/completed");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetIncompleteTodos_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/todos/incomplete");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SecurityHeaders_ShouldBePresent()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/todos");

        // Assert
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").First());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").First());
        Assert.Equal("1; mode=block", response.Headers.GetValues("X-XSS-Protection").First());
        Assert.Equal("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").First());
        Assert.Equal("default-src 'self'", response.Headers.GetValues("Content-Security-Policy").First());
    }
}
