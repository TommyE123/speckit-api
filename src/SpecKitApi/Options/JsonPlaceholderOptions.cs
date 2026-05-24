namespace SpecKitApi.Options;

/// <summary>
/// Configuration options for the JSONPlaceholder HTTP client.
/// </summary>
public sealed class JsonPlaceholderOptions
{
    /// <summary>
    /// The configuration section name used to bind this options class.
    /// </summary>
    public const string SectionName = "JsonPlaceholderOptions";

    /// <summary>
    /// The base URL of the JSONPlaceholder API.
    /// </summary>
    public string BaseUrl { get; init; } = string.Empty;
}
