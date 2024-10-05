// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text.RegularExpressions;

using Discord;
using Discord.Interactions;

using Giorgione.Config;

using Microsoft.Extensions.Logging;

namespace Giorgione.Modules;

public partial class StarboardModule(IHttpClientFactory clientFactory, ILogger<StarboardModule> logger, BotConfig config)
    : BotModule(logger)
{
    private static readonly Random random = new();
    private static readonly Regex regex = urlRegex();

    private ITextChannel? _starboard;

    /// <inheritdoc />
    public override async Task BeforeExecuteAsync(ICommandInfo command)
    {
        _starboard ??= (ITextChannel) await Context.Client.GetChannelAsync(config.StarboardId).ConfigureAwait(false);
    }

    [MessageCommand("Star")]
    public async Task CloneToStarboard(IMessage message)
    {
        if (_starboard is null)
        {
            await RespondAsync("Canale non trovato. Sicuro che non l'avete obliterato?", ephemeral: true);
            return;
        }

        if (message is not IUserMessage userMessage)
        {
            await RespondAsync("Fanculo il sistema! No stelline a Discord :face_with_symbols_over_mouth:", ephemeral: true);
            return;
        }

        if (userMessage.Author.IsBot || userMessage.Author.IsWebhook)
        {
            await RespondAsync("Ao ma che me stai a pija per culo :angry:", ephemeral: true);
            return;
        }

        await DeferAsync(ephemeral: true);

        var embedColor = getRandomColor();

        var (imgUrls, attachments) = checkUrls(userMessage);

        string cleanMessage = stripImgAttachments(userMessage, imgUrls);

        string iconUrl = userMessage.Author.GetDisplayAvatarUrl();

        var mainContent = new EmbedBuilder()
            .WithColor(embedColor)
            .WithAuthor(author =>
            {
                author.WithName($"{userMessage.Author.Username}  •  {userMessage.Author.Id}");
                author.WithIconUrl(iconUrl);
            })
            .WithTitle("Torna al messaggio")
            .WithUrl(userMessage.GetJumpUrl())
            .WithFooter($"ID: {userMessage.Id}")
            .WithTimestamp(userMessage.Timestamp);

        if (!string.IsNullOrEmpty(cleanMessage))
        {
            mainContent.WithDescription(cleanMessage);
        }

        if (attachments.Count > 0)
        {
            mainContent.WithFields(new EmbedFieldBuilder()
                .WithName("Allegati")
                .WithValue(string.Join('\n', attachments)));
        }

        if (imgUrls.Count > 0)
        {
            mainContent.WithImageUrl(imgUrls[0]);
        }

        await _starboard!.SendMessageAsync(embed: mainContent.Build());

        //Send the images in different embeds
        if (imgUrls.Count > 1)
        {
            await Parallel.ForEachAsync(imgUrls.Skip(1), async (url, ct) =>
            {
                var embed = new EmbedBuilder()
                    .WithColor(embedColor)
                    .WithAuthor(
                        name: $"{userMessage.Author.Username}  •  {userMessage.Author.Id}",
                        iconUrl: iconUrl)
                    .WithImageUrl(url)
                    .WithFooter($"ID: {userMessage.Id}")
                    .WithTimestamp(userMessage.Timestamp)
                    .Build();

                await _starboard!.SendMessageAsync(embed: embed);
            });
        }

        Logger.LogInformation("Starboard :: Message <{MessageId}> has been starred", message.Id);

        await ModifyOriginalResponseAsync(properties => properties.Content = "Il messaggio è stato stellinato!");
    }

    private string stripImgAttachments(IMessage message, ICollection<string> imgUrls)
    {
        return regex
            .Replace(message.Content, match =>
            {
                // Do not remove the url if it's not an image
                if (!isImage(match.Value))
                    return match.Value;

                imgUrls.Add(match.Value);
                return string.Empty;
            })
            .Trim();
    }

    private (List<string> imgUrls, List<string> attachments) checkUrls(IMessage message)
    {
        List<string> imgUrls = [];
        List<string> attachments = [];

        Parallel.ForEach(message.Attachments.Select(a => a.Url), url =>
        {
            if (isImage(url)) imgUrls.Add(url);

            else attachments.Add(url);
        });

        return (imgUrls, attachments);
    }

    private bool isImage(string url)
    {
        using var client = clientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Head, url);
        using var response = client.Send(request);

        return response.IsSuccessStatusCode &&
               (response.Content.Headers.ContentType?.MediaType?.StartsWith("image/") ?? false);
    }

    private static Color getRandomColor()
    {
        const int maxValue = (int) Color.MaxDecimalValue;

        return new Color((uint) random.Next(maxValue));
    }

    [GeneratedRegex(@"(http[s]?://(?:[a-zA-Z]|[0-9]|[$-_@.&+]|[!*\\(\\),]|(?:%[0-9a-fA-F][0-9a-fA-F]))+)")]
    private static partial Regex urlRegex();
}
