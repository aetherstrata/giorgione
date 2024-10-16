// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Discord;
using Discord.WebSocket;

using Giorgione.Config;
using Giorgione.Data;
using Giorgione.Data.Filters;
using Giorgione.Data.Models;

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
        var results = await getCelebratingUsers();

        if (results.Count == 0) return;

        string message = string.Join('\n', results.Select(mapGreetingMessage));
        var channel = (ITextChannel) client.GetGuild(config.GuildId).GetChannel(712271135021989938);
        await channel.SendMessageAsync(message);
    }

    private async Task<List<User>> getCelebratingUsers()
    {
        logger.LogDebug("Checking for birthdays...");

        await using var db = await dbFactory.CreateDbContextAsync();

        var results = await db.Users
            .Where(UserFilter.ByBirthdate(DateTime.Now))
            .ToListAsync();

        logger.LogDebug("Found {Count} users", results.Count);

        return results;
    }

    private static string mapGreetingMessage(User user) => user.Birthdate switch
    {
        FullDate fullDate => $"Auguri a {user.ToMentionString()}, che oggi compie {fullDate.GetAge()} anni!",
        MonthDay => $"Auguri a {user.ToMentionString()}!",
        _ => throw new ArgumentOutOfRangeException(nameof(user),"User with an unexpected kind of birthdate.")
    };
}
