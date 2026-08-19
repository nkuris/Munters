namespace Munters.Server.Models
{
    /// <summary>
    /// Represents a GIF result with its ID, title, URL, and preview URL.
    /// </summary>
    /// <param name="Id">The ID of the GIF.</param>
    /// <param name="Title">The title of the GIF.</param>
    /// <param name="Url">The URL of the GIF.</param>
    /// <param name="PreviewUrl">The preview URL of the GIF.</param>
    public record GifResultDto(
      string Id,
      string Title,
      string Url,
      string PreviewUrl
  );
}
