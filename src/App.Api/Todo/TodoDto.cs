namespace App.Api.Todo;

/// <summary>
/// Represents a to-do item returned by the API.
/// </summary>
internal record TodoResponse(
    int Id,
    string Title,
    bool IsCompleted,
    DateOnly? DueBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

/// <summary>
/// Represents a request to create a new to-do item.
/// </summary>
internal record CreateTodoRequest(string Title, bool IsCompleted, DateOnly? DueBy);

/// <summary>
/// Represents a request to update an existing to-do item.
/// </summary>
internal record UpdateTodoRequest(string Title, bool IsCompleted, DateOnly? DueBy);
