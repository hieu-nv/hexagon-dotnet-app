namespace App.Api.Admin;

/// <summary>
/// Endpoint handler for admin-only operations.
/// All endpoints in this class require the <c>AdminOnly</c> authorization policy.
/// </summary>
internal sealed class AdminEndpoints(ILogger<AdminEndpoints> logger)
{
    private readonly ILogger<AdminEndpoints> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Returns system statistics for administrators.
    /// Requires the <c>admin</c> role.
    /// </summary>
    public IResult GetStats()
    {
        _logger.LogInformation("Admin stats endpoint accessed");

        // In a real system these would be pulled from monitoring/database.
        var stats = new AdminStatsResponse(
            Status: "healthy",
            Timestamp: DateTimeOffset.UtcNow,
            Version: typeof(AdminEndpoints).Assembly.GetName().Version?.ToString() ?? "unknown",
            Notes: "Administrative statistics endpoint. Data is illustrative.");

        _logger.LogInformation("Admin stats returned successfully");
        return Results.Ok(stats);
    }
}

/// <summary>Response DTO for <c>GET /admin/stats</c>.</summary>
internal sealed record AdminStatsResponse(
    string Status,
    DateTimeOffset Timestamp,
    string Version,
    string Notes);
