// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Discord;
using Discord.WebSocket;

using Giorgione.Config;
using Giorgione.Data;
using Giorgione.Data.Filters;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Quartz;

namespace Giorgione.Workers;

internal class BirthdateChecker(
    DiscordSocketClient client,
    BotConfig config,
    ILogger<BirthdateChecker> logger,
    IDbContextFactory<AppDbContext> dbFactory) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogDebug("Checking for birthdays...");

        await using var db = await dbFactory.CreateDbContextAsync();

        var results = await db.Users
            .Where(UserFilter.ByBirthdate(DateTime.Now))
            .ToListAsync();

        logger.LogDebug("Found {Count} users", results.Count);

        if (results.Count > 0)
        {
            var channel = (ITextChannel) client.GetGuild(config.GuildId).GetChannel(712271135021989938);
            string userMentions = string.Join(' ', results.Select(user => user.ToMentionString()));

            await channel.SendMessageAsync($"Auguri a {userMentions}!");
        }
    }
}
