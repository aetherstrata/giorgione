// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text.RegularExpressions;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Giorgione.Data;
using Giorgione.Data.Extensions;
using Giorgione.Data.Models;

using Microsoft.Extensions.Logging;

namespace Giorgione.Modules;

[Group("starboard", "Manage the starboard")]
public partial class StarboardModule(
    AppDbContext db,
    HttpClient http,
    ILogger<StarboardModule> logger)
    : BotModule(logger)
{
    private static readonly Random random = new();
    private static readonly Regex regex = urlRegex();

    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [SlashCommand("enable", "Enable the starboard")]
    public async Task EnableStarboardAsync(ITextChannel channel)
    {
        bool changed = await db.UpsertAsync(x => x.Id == Context.Guild.Id, new Guild(Context.Guild.Id), guild =>
        {
            bool res = guild.StarboardId.HasValue;

            guild.StarboardId = channel.Id;

            return res;
        });

        if (changed)
            await RespondAsync($"Starboard changed! Starred messages will now be posted in {channel.Mention}.");
        else
            await RespondAsync($"Starboard enabled! Starred messages will be posted in {channel.Mention}.");
    }

    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [SlashCommand("disable", "Disable the starboard")]
    public async Task DisableStarboardAsync()
    {
        bool hadValue = await db.UpsertAsync(x => x.Id == Context.Guild.Id, new Guild(Context.Guild.Id), guild =>
        {
            bool res = guild.StarboardId.HasValue;

            guild.StarboardId = null;

            return res;
        });

        if (hadValue)
            await RespondAsync("Starboard disabled! Starred messages will not be posted anymore.");
        else
            await RespondError("Command failed","Starboard disabled already, nothing to do.");
    }

    [MessageCommand("Star")]
    public async Task CloneToStarboard(IMessage message)
    {
        if (message is not IUserMessage userMessage)
        {
            await RespondAsync("Fanculo il sistema! No stelline a Discord :face_with_symbols_over_mouth:",
                ephemeral: true);
            return;
        }

        if (userMessage.Author.IsBot || userMessage.Author.IsWebhook)
        {
            await RespondAsync("Ao ma che me stai a pija per culo :angry:", ephemeral: true);
            return;
        }

        var channel = await getStarboard();

        if (channel == null) return;

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

        await channel.SendMessageAsync(embed: mainContent.Build());

        //Send the images in different embeds
        if (imgUrls.Count > 1)
        {
            await Parallel.ForEachAsync(imgUrls.Skip(1), async (url, _) =>
            {
                var embed = new EmbedBuilder()
                    .WithColor(embedColor)
                    .WithAuthor(author =>
                    {
                        author.WithName($"{userMessage.Author.Username}  •  {userMessage.Author.Id}");
                        author.WithIconUrl(iconUrl);
                    })
                    .WithImageUrl(url)
                    .WithFooter($"ID: {userMessage.Id}")
                    .WithTimestamp(userMessage.Timestamp)
                    .Build();

                await channel.SendMessageAsync(embed: embed);
            });
        }

        Logger.LogInformation("Starboard :: Message <{MessageId}> has been starred", message.Id);

        await ModifyOriginalResponseAsync(properties => properties.Content = "Il messaggio è stato stellinato!");
    }

    private string stripImgAttachments(IMessage message, List<string> imgUrls)
    {
        ArgumentNullException.ThrowIfNull(imgUrls);

        return regex
            .Replace(message.Content, match =>
            {
                // Do not remove the url if it's not an image
                if (http.IsImage(match.Value))
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
            if (http.IsImage(url)) imgUrls.Add(url);

            else attachments.Add(url);
        });

        return (imgUrls, attachments);
    }

    private async Task<ITextChannel?> getStarboard()
    {
        var guild = await db.Guilds.FindAsync(Context.Guild.Id) ?? new Guild(Context.Guild.Id);

        if (!guild.StarboardId.HasValue)
        {
            await RespondError("Starboard is disabled", "Please execute `/starboard enable <channel>` to start starring messages.");
            return null;
        }

        var channel = Context.Guild.GetTextChannel(guild.StarboardId.Value);

        if (channel == null)
        {
            await RespondError("Starboard was not found", "Please execute `/starboard enable <channel>` to set a new starboard channel.");
            return null;
        }

        return channel;
    }

    private static Color getRandomColor()
    {
        const int maxValue = (int)Color.MaxDecimalValue;

        return new Color((uint)random.Next(maxValue));
    }

    [GeneratedRegex(@"(http[s]?://(?:[a-zA-Z]|[0-9]|[$-_@.&+]|[!*\\(\\),]|(?:%[0-9a-fA-F][0-9a-fA-F]))+)")]
    private static partial Regex urlRegex();
}
