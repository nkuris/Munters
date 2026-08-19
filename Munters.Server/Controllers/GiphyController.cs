using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Munters.Server.Models;
using Munters.Server.Services;

namespace Munters.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GiphyController : ControllerBase
    {
        private readonly IGiphyService _giphyService;
        private readonly ILogger<GiphyController> _logger;
        private readonly IWebHostEnvironment _env;

        public GiphyController(IGiphyService giphyService, ILogger<GiphyController> logger, IWebHostEnvironment env)
        {
            _giphyService = giphyService;
            _logger = logger;
            _env = env;
        }

        /// <summary>
        /// Fetches trending GIFs of the day.
        /// </summary>
        [HttpGet("trending")]
        public async Task<ActionResult<IEnumerable<GifResultDto>>> GetTrending([FromQuery] int limit = 25,
            CancellationToken cancellationToken = default)
        {
            
            _logger.LogInformation("Fetching trending GIFs.");
            try
            {


                var gifs = await _giphyService.GetTrendingAsync(limit, cancellationToken);
                return Ok(gifs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching trending GIFs.");
                // In Development surface exception details to help debugging, otherwise return generic message
                if (_env.IsDevelopment())
                    return Problem(detail: ex.Message, title: "Error fetching trending GIFs");

                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching trending GIFs." });
            }
        }

        /// <summary>
        /// Searches GIFs by a input search query.
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<GifResultDto>>> Search([FromQuery] string q, [FromQuery] int limit = 25, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Searching GIFs with query: {Query}", q);
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return BadRequest(new { message = "Search query parameter 'q' cannot be empty." });

                var gifs = await _giphyService.SearchAsync(q, limit, cancellationToken);
                return Ok(gifs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching GIFs with query: {Query}", q);
                if (_env.IsDevelopment())
                    return Problem(detail: ex.Message, title: "Error searching GIFs");

                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while searching GIFs." });
            }
        }
    }
}
