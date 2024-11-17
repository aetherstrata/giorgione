// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Discord;
using Discord.Interactions;

using Giorgione.Data;
using Giorgione.Data.Extensions;

using Microsoft.Extensions.Logging;

namespace Giorgione.Modules;

[Group("aw", "Manage AnimeWorld feed")]
public class AnimeWorldModule(AppDbContext db, ILogger<AnimeWorldModule> logger) : BotModule(logger)
{
    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [SlashCommand("enable", "Enable the new episodes feed")]
    public async Task EnableAnimeFeed(ITextChannel channel)
    {
        bool changed = await db.Guilds.UpsertAsync(Context.Guild.Id, guild =>
        {
            bool res = guild.AnimeFeedChannelId.HasValue;

            guild.AnimeFeedChannelId = channel.Id;

            return res;
        });

        if (changed)
            await RespondAsync($"Channel changed! New episodes will now be posted in {channel.Mention}.");
        else
            await RespondAsync($"Feed enabled! New episodes will be posted in {channel.Mention}.");
    }

    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [SlashCommand("disable", "Disable the new episodes feed")]
    public async Task DisableAnimeFeed()
    {
        bool hadValue = await db.Guilds.UpsertAsync(Context.Guild.Id, guild =>
        {
            bool res = guild.AnimeFeedChannelId.HasValue;

            guild.AnimeFeedChannelId = null;

            return res;
        });

        if (hadValue)
            await RespondAsync("Anime feed disabled! New episodes will not be posted anymore.");
        else
            await RespondError("Command failed","The anime feed is disabled already, nothing to do.");
    }
}
