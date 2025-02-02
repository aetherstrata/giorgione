// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ServiceModel.Syndication;
using System.Xml;

using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Giorgione.Api.AnimeWorld;

public class AnimeWorldClient
{
    private const string rss_url = "http://www.animeworld.so/rss/episodes";
    private const string cookie_file = "./aw_security_cookie";

    private static readonly SyndicationFeed empty_feed = new();

    private readonly HttpClient _http;
    private readonly ILogger<AnimeWorldClient> _logger;

    public AnimeWorldClient(ILogger<AnimeWorldClient> logger, HttpClient httpClient)
    {
        _logger = logger;
        _http = httpClient;

        if (!File.Exists(cookie_file))
        {
            File.Create(cookie_file).Close();
        }
    }

    public async IAsyncEnumerable<AnimeWorldEpisode> GetEpisodes()
    {
        _logger.LogDebug("Reading AnimeWorld feed");

        var feed = await getRssFeed();

        _logger.LogDebug("Got feed successfully. Last updated: {PubDate}", feed.LastUpdatedTime);

        foreach (var item in feed.Items)
        {
            yield return parseEpisode(item);
        }
    }

    private async Task<SyndicationFeed> getRssFeed()
    {
        for (int retry = 0; retry < 5; retry++) try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, rss_url);

            // Add the security cookie to request header
            string securityCookie = await File.ReadAllTextAsync(cookie_file);
            if (!string.IsNullOrEmpty(securityCookie))
            {
                request.Headers.Add(HeaderNames.Cookie, securityCookie);
            }

            using var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();

            // Get the cookie for the next request
            string? newCookie = content.StartsWith("<html>", StringComparison.Ordinal) switch
            {
                true => getCookieFromContent(content),
                false => getCookieFromHeaders(response)
            };

            if (!string.IsNullOrEmpty(newCookie))
            {
                await File.WriteAllTextAsync(cookie_file, newCookie);
            }

            return SyndicationFeed.Load(XmlReader.Create(new StringReader(content)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving AnimeWorld feed. Try #{RetryNumber}", retry);
        }

        return empty_feed;
    }

    private static string? getCookieFromHeaders(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies)) return null;

        string? newCookie = cookies.FirstOrDefault(x => x.StartsWith("SecurityAW", StringComparison.Ordinal));

        return !string.IsNullOrEmpty(newCookie)
            ? newCookie[..newCookie.IndexOf(';')]
            : null;
    }

    private static string? getCookieFromContent(string content)
    {
        int begin = content.IndexOf("SecurityAW", StringComparison.Ordinal);
        int end = content.IndexOf(';', begin);

        return content[begin..end].Trim();
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

        ep.EpisodeNumber = isDouble
            ? new DoubleEpNumber(double.Parse(episode))
            : new IntEpNumber(int.Parse(episode));

        return ep;
    }
}
