
namespace Giorgione.Api.AnimeWorld;

public class AnimeWorldEpisode
{
    public required int Guid { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public required Uri EpisodeUrl { get; set; }

    public required DateTimeOffset PublicationDate { get; set; }

    public EpisodeNumber EpisodeNumber { get; set; } = new NumberNotAvailable();

    public string? AnimeName { get; set; }

    public string? JapName { get; set; }

    public EpisodeCount TotalEpisodes { get; set; } = new CountNotAvailable();

    public Uri? AnimeUrl { get; set; }

    public bool IsDub { get; set; }

    public Uri? ImageUrl { get; set; }

    public Uri? CoverUrl { get; set; }
}
