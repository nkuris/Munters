using Munters.Server.Models;

namespace Munters.Server.Services
{
    public interface IGiphyService
    {
        Task<IEnumerable<GifResultDto>> GetTrendingAsync(int limit = 25, CancellationToken cancellationToken = default);
        Task<IEnumerable<GifResultDto>> SearchAsync(string query, int limit = 25, CancellationToken cancellationToken = default);
    }
}
