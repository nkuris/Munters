using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Munters.Server.Models;

namespace Munters.Server.Services
{
    public class CachedGiphyService : IGiphyService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedGiphyService"/> class.
        /// </summary>
        private readonly IGiphyService _innerService;


        /// <summary>
        /// Memory cache instance for caching Giphy results.
        /// </summary>
        private readonly IMemoryCache _cache;
        /// <summary>
        /// the cache duratio
        /// </summary>
        private readonly TimeSpan _cacheDuration;
        private readonly ILogger<CachedGiphyService> _logger;

        /// <summary>
        /// CachedGiphyService constructor
        /// </summary>
        /// <param name="innerService">inner IGiphyService</param>
        /// <param name="cache">IMemoryCache instance</param>
        /// <param name="cacheDuration">cache duration timespan</param>
        /// <param name="logger">logger instance</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CachedGiphyService(IGiphyService innerService, IMemoryCache cache, TimeSpan cacheDuration, ILogger<CachedGiphyService> logger)
        {
            _innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _cacheDuration = cacheDuration;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets trending GIFs from Giphy, with caching to reduce API calls.
        /// </summary>
        /// <param name="limit">The maximum number of trending GIFs to retrieve.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A list of trending GIFs.</returns>
        public async Task<IEnumerable<GifResultDto>> GetTrendingAsync(int limit = 25, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"giphy_trending_limit_{limit}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<GifResultDto>? cached) && cached is not null)
            {
                _logger.LogInformation("Cache hit for key {CacheKey}", cacheKey);
                return cached;
            }

            _logger.LogInformation("Cache miss for key {CacheKey}. Fetching from Giphy.", cacheKey);

            var result = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                var fetched = await _innerService.GetTrendingAsync(limit, cancellationToken);
                // Log after successful fetch
                _logger.LogInformation("Cached {Count} items for key {CacheKey} (expires in {Minutes}m)", fetched?.Count() ?? 0, cacheKey, _cacheDuration.TotalMinutes);
                return fetched;
            });

            return result!;


        }

        public async Task<IEnumerable<GifResultDto>> SearchAsync(string query, int limit = 25, CancellationToken cancellationToken = default)
        {
            // Normalize input query key to treat 'cat', 'Cat ', and 'CAT' as identical
            var normalizedQuery = query.Trim().ToLowerInvariant();
            var cacheKey = $"giphy_search_{normalizedQuery}_limit_{limit}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<GifResultDto>? cached) && cached is not null)
            {
                _logger.LogInformation("Cache hit for key {CacheKey}", cacheKey);
                return cached;
            }

            _logger.LogInformation("Cache miss for key {CacheKey}. Fetching from Giphy.", cacheKey);

            var result = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                var fetched = await _innerService.SearchAsync(query, limit, cancellationToken);
                _logger.LogInformation("Cached {Count} items for key {CacheKey} (expires in {Minutes}m)", fetched?.Count() ?? 0, cacheKey, _cacheDuration.TotalMinutes);
                return fetched;
            });

            return result!;
        }
    }
}
