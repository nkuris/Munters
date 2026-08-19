namespace Munters.Server.Models
{
    /// <summary>
    /// Strongly-typed settings for Giphy integration.
    /// Configure via the "Giphy" section in appsettings or user-secrets.
    /// </summary>
    public record GiphyOptions
    {
        /// <summary>
        /// The API key for accessing the Giphy API. This should be set in configuration or user-secrets.
        /// </summary>
        public string? ApiKey { get; init; }
        /// <summary>
        /// The base URL for the Giphy API. Defaults to "https://api.giphy.com/" if not set.
        /// </summary>
        public string? BaseUrl { get; init; }
        /// <summary>
        /// The duration in minutes to cache both trending and search results. Defaults to 30 minutes.
        /// </summary>
        public int CacheDurationMinutes { get; init; } = 30;
    }
}
