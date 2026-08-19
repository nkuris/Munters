using Munters.Server.Models;
using Microsoft.Extensions.Options;
using System;
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
            _apiKey = !string.IsNullOrEmpty(trimmed)
                ? trimmed
                : throw new InvalidOperationException("Giphy API key is not configured. Set Giphy:ApiKey in configuration or user-secrets.");
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
                // Include body in exception to aid debugging (may contain error details from Giphy)
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
