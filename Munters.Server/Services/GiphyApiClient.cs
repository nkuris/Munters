using Munters.Server.Models;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Munters.Server.Services
{
    public class GiphyApiClient : IGiphyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly Microsoft.Extensions.Logging.ILogger<GiphyApiClient> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GiphyApiClient"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="options">Bound Giphy options (ApiKey, BaseUrl).</param>
        public GiphyApiClient(HttpClient httpClient, IOptions<GiphyOptions> options, Microsoft.Extensions.Logging.ILogger<GiphyApiClient> logger)
        {
            _httpClient = httpClient;
            // Trim whitespace/newlines from API key sourced from environment or files
            var rawKey = options?.Value?.ApiKey;
            var trimmed = rawKey?.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                _apiKey = trimmed;
            }
            else
            {
                throw new InvalidOperationException(
                    "Giphy API key is not configured.\n" +
                    "Set 'Giphy:ApiKey' in configuration (appsettings.json), provide the environment variable 'GIPHY__APIKEY', or use user-secrets: `dotnet user-secrets set \"Giphy:ApiKey\" \"<your-key>\"`.\n" +
                    "Get a key at https://developers.giphy.com/.");
            }
            _logger = logger;
        }
        public async Task<IEnumerable<GifResultDto>> GetTrendingAsync(int limit = 25, CancellationToken cancellationToken = default)
        {
            var endpoint = $"v1/gifs/trending?api_key={_apiKey}&limit={limit}&rating=g";
            return await FetchFromGiphyAsync(endpoint, cancellationToken);
        }

        public async Task<IEnumerable<GifResultDto>> SearchAsync(string query, int limit = 25, CancellationToken cancellationToken = default)
        {
            var encodedQuery = Uri.EscapeDataString(query);
            var endpoint = $"v1/gifs/search?api_key={_apiKey}&q={encodedQuery}&limit={limit}&rating=g";
            return await FetchFromGiphyAsync(endpoint, cancellationToken);
        }

        private async Task<IEnumerable<GifResultDto>> FetchFromGiphyAsync(string endpoint, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Giphy request: {Endpoint} (key suffix: {KeySuffix})", endpoint, _apiKey.Length > 4 ? _apiKey[^4..] : _apiKey);
            using var resp = await _httpClient.GetAsync(endpoint, cancellationToken);

            var content = await resp.Content.ReadAsStringAsync(cancellationToken);

            if (!resp.IsSuccessStatusCode)
            {
                // If Giphy returns 401/403, surface a clear message that the API key is missing or invalid
                if (resp.StatusCode == HttpStatusCode.Unauthorized || resp.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new InvalidOperationException($"Giphy authentication failed ({(int)resp.StatusCode} {resp.ReasonPhrase}). The configured API key may be missing or invalid.\n" +
                        "Set 'Giphy:ApiKey' in appsettings.json, the environment variable 'GIPHY__APIKEY', or use user-secrets: `dotnet user-secrets set \"Giphy:ApiKey\" \"<your-key>\"`. Get a key at https://developers.giphy.com/.\n" +
                        $"Response body: {content}");
                }

                // If the response body mentions the api key, offer the same guidance to help the developer
                var lower = content?.ToLowerInvariant() ?? string.Empty;
                if (lower.Contains("api key") || lower.Contains("api_key") || lower.Contains("invalid api") || lower.Contains("authentication"))
                {
                    throw new HttpRequestException($"Giphy returned {(int)resp.StatusCode} {resp.ReasonPhrase}: {content}\n" +
                        "This may indicate a missing or invalid API key. Set 'Giphy:ApiKey' in appsettings.json, the environment variable 'GIPHY__APIKEY', or use user-secrets: `dotnet user-secrets set \"Giphy:ApiKey\" \"<your-key>\"`. Get a key at https://developers.giphy.com/.");
                }

                // Otherwise throw a generic HttpRequestException including the response body to aid debugging
                throw new HttpRequestException($"Giphy returned {(int)resp.StatusCode} {resp.ReasonPhrase}: {content}");
            }

            var response = System.Text.Json.JsonSerializer.Deserialize<GiphyResponse>(content, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (response?.Data == null)
                return Enumerable.Empty<GifResultDto>();

            return response.Data.Select(g => new GifResultDto(
                Id: g.Id,
                Title: g.Title,
                Url: g.Images.Original.Url,
                PreviewUrl: g.Images.FixedHeight.Url
            ));
        }
    }
}
