// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ServiceModel.Syndication;
using System.Xml;

using Microsoft.Extensions.Logging;

namespace Giorgione.Api.AnimeWorld;

public class AnimeWorldClient
{
    private const string rss_url = "http://www.animeworld.so/rss/episodes";

    private readonly ILogger<AnimeWorldClient> _logger;

    public AnimeWorldClient(ILogger<AnimeWorldClient> logger)
    {
        _logger = logger;
    }

    public IEnumerable<AnimeWorldEpisode> GetFeed()
    {
        _logger.LogDebug("Reading AnimeWorld feed");

        using var reader = XmlReader.Create(rss_url);
        var feed = SyndicationFeed.Load(reader);

        _logger.LogDebug("Got feed successfully. Last updated: {PubDate}", feed.LastUpdatedTime);

        foreach (var item in feed.Items)
        {
            yield return parseEpisode(item);
        }
    }

    private static AnimeWorldEpisode parseEpisode(SyndicationItem item)
    {
        var ep = new AnimeWorldEpisode
        {
            Guid = int.Parse(item.Id),
            Title = item.Title.Text,
            Description = item.Summary.Text,
            EpisodeUrl = item.Links[0].Uri,
            PublicationDate = item.PublishDate,
        };

        var reader = item.ElementExtensions.GetReaderAtElementExtensions();

        bool isDouble = false;
        string episode = string.Empty;

        do
        {
            if (reader.NodeType != XmlNodeType.Element) continue;

            switch (reader.LocalName)
            {
                case "name":
                    ep.AnimeName = reader.ReadElementContentAsString();
                    break;

                case "jtitle":
                    ep.JapName = reader.ReadElementContentAsString();
                    break;

                case "episodes":
                    string countString = reader.ReadElementContentAsString();

                    ep.TotalEpisodes = countString == "??"
                        ? new CountNotAvailable()
                        : new CountAvailable(int.Parse(countString));
                    break;

                case "link":
                    ep.AnimeUrl = new Uri(reader.ReadElementContentAsString());
                    break;

                case "dub":
                    ep.IsDub = reader.ReadElementContentAsBoolean();
                    break;

                case "image":
                    ep.ImageUrl = new Uri(reader.ReadElementContentAsString());
                    break;

                case "cover":
                    ep.CoverUrl = new Uri(reader.ReadElementContentAsString());
                    break;

                case "double":
                    isDouble = reader.ReadElementContentAsBoolean();
                    break;

                case "number":
                    episode = reader.ReadElementContentAsString();
                    break;
            }
        }
        while (reader.Read());

        ep.EpisodeNumber = isDouble ? new DoubleEpNumber(double.Parse(episode)) : new IntEpNumber(int.Parse(episode));

        return ep;
    }
}
