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
            var fetchedEpisodes = await aw.GetEpisodes().ToArrayAsync().ConfigureAwait(false);

            var fetchedIds = fetchedEpisodes
                .Select(x => x.Guid)
                .ToHashSet();

            var seenIds = await db.SeenEpisodes
                .AsNoTracking()
                .Where(ep => fetchedIds.Contains(ep.Id))
                .Select(ep => ep.Id)
                .ToHashSetAsync();

            var newEpisodes = fetchedEpisodes.Where(ep => !seenIds.Contains(ep.Guid)).ToArray();

            if (newEpisodes.Length == 0) return;

            await db.SeenEpisodes.AddRangeAsync(newEpisodes.Select(ep => new SeenEpisode(ep.Guid)));
            await db.SaveChangesAsync();

            var channelIds = await db.Guilds
                .AsNoTracking()
                .Where(g => g.AnimeFeedChannelId.HasValue)
                .Select(g => g.AnimeFeedChannelId!.Value)
                .ToListAsync();

            await Parallel.ForEachAsync(channelIds, async (channelId, ct) =>
            {
                var channel = await discord.GetChannelAsync(channelId).ConfigureAwait(false) as ITextChannel;

                var embedChunks = newEpisodes.Select(ep => new EmbedBuilder()
                        .WithColor(Color.Blue)
                        .WithTitle(ep.Title)
                        .WithUrl(ep.EpisodeUrl.AbsoluteUri)
                        .WithImageUrl(ep.CoverUrl?.AbsoluteUri)
                        .WithThumbnailUrl(ep.ImageUrl?.AbsoluteUri)
                        .WithTimestamp(ep.PublicationDate)
                        .Build())
                    .Chunk(10)
                    .ToArray();

                foreach (var chunk in embedChunks)
                {
                    await channel!.SendMessageAsync(embeds: chunk).ConfigureAwait(false);
                }
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while processing AW rss feed");
        }
    }
}
