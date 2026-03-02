namespace App.Core.Poke;

/// <summary>
/// Represents a Pokemon entity.
/// </summary>
public class Pokemon
{
    /// <summary>
    /// Gets or sets the name of the Pokemon.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL for detailed Pokemon information.
    /// </summary>
    public string Url { get; set; } = string.Empty;
}
