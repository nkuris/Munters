using System.Runtime.InteropServices.ComTypes;
using System.Text.Json.Serialization;

namespace Munters.Server.Models
{
    public record GiphyResponse([property: JsonPropertyName("data")] List<GifDataObject> Data);

    public record GifDataObject(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("images")] ImageRenditions Images
    );

    public record ImageRenditions(
    [property: JsonPropertyName("original")] ImageUrl Original,
    [property: JsonPropertyName("fixed_height")] ImageUrl FixedHeight
);

    public record ImageUrl(
    [property: JsonPropertyName("url")] string Url
);
}
