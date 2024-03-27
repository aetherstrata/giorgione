// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text.RegularExpressions;

using Discord;
using Discord.Interactions;

using Giorgione.Config;

using Serilog;

namespace Giorgione.Modules;

public partial class Starboard(IHttpClientFactory clientFactory, BotConfig config) : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Random random = new();
    private static readonly Regex regex = urlRegex();

    private ITextChannel? _starboard;

    /// <inheritdoc />
    public override async Task BeforeExecuteAsync(ICommandInfo command)
    {
        _starboard ??= (ITextChannel) await Context.Client.GetChannelAsync(config.StarboardId).ConfigureAwait(false);
    }

    [MessageCommand("Stellina")]
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

        var embedColor = getRandomColor();

        var (imgUrls, attachments) = checkUrls(userMessage);

        string cleanMessage = stripImgAttachments(userMessage, imgUrls);

        string iconUrl = userMessage.Author.GetAvatarUrl() ?? userMessage.Author.GetDefaultAvatarUrl();

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
            await Parallel.ForEachAsync(imgUrls.Skip(1), async (url, _) =>
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

        Log.Information("Starboard :: Message <{MessageId}> has been starred", message.Id);

        await RespondAsync("Il messaggio è stato stellinato!", ephemeral: true);
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
        var imgUrls = new List<string>();
        var attachments = new List<string>();

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

        if (response.IsSuccessStatusCode)
        {
            return response.Content.Headers.ContentType?.MediaType?.StartsWith("image/") ?? false;
        }

        return false;
    }

    private static Color getRandomColor()
    {
        return new Color((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
    }

    [GeneratedRegex(@"(http[s]?://(?:[a-zA-Z]|[0-9]|[$-_@.&+]|[!*\\(\\),]|(?:%[0-9a-fA-F][0-9a-fA-F]))+)")]
    private static partial Regex urlRegex();
}
