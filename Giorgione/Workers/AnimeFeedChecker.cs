// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Discord;
using Discord.WebSocket;

using Giorgione.Data;
using Giorgione.Data.Models;
using Giorgione.Api.AnimeWorld;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Quartz;

namespace Giorgione.Workers;

public class AnimeFeedChecker(AnimeWorldClient aw, AppDbContext db, DiscordSocketClient discord, ILogger<AnimeFeedChecker> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var fetchedEpisodes = aw.GetFeed().ToArray();

            //TODO: Change to ToHashSetAsync when Npgsql is updated to EF Core 9
            var seenIds = db.SeenEpisodes
                .Where(ep => fetchedEpisodes
                    .Select(x => x.Guid)
                    .ToHashSet()
                    .Contains(ep.Id))
                .Select(ep => ep.Id)
                .ToHashSet();

            var newEpisodes = fetchedEpisodes.Where(ep => !seenIds.Contains(ep.Guid)).ToArray();

            await db.SeenEpisodes.AddRangeAsync(newEpisodes.Select(ep => new SeenEpisode(ep.Guid)));

            await db.SaveChangesAsync();

            var channelIds = await db.Guilds
                .Where(g => g.AnimeFeedChannelId.HasValue)
                .Select(g => g.AnimeFeedChannelId!.Value)
                .ToListAsync();

            await Parallel.ForEachAsync(channelIds, async (channelId, ct) =>
            {
                var channel = await discord.GetChannelAsync(channelId).ConfigureAwait(false) as ITextChannel;

                var embeds = newEpisodes.Select(ep => new EmbedBuilder()
                        .WithColor(Color.Blue)
                        .WithTitle(ep.Title)
                        .WithUrl(ep.EpisodeUrl.AbsoluteUri)
                        .WithImageUrl(ep.CoverUrl?.AbsoluteUri)
                        .WithThumbnailUrl(ep.ImageUrl?.AbsoluteUri)
                        .WithTimestamp(ep.PublicationDate)
                        .Build())
                    .ToArray();

                await channel!.SendMessageAsync(embeds: embeds).ConfigureAwait(false);
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while processing AW rss feed");
        }
    }
}
