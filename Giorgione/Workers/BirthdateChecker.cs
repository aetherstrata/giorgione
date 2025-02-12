// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Discord;
using Discord.WebSocket;

using Giorgione.Data;
using Giorgione.Data.Filters;
using Giorgione.Data.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Quartz;

namespace Giorgione.Workers;

internal class BirthdateChecker(
    AppDbContext db,
    DiscordSocketClient client,
    ILogger<BirthdateChecker> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var guilds = await db.Members.WithBirthdate(DateTime.Now)
            .GroupBy(m => m.Guild, m => m.User)
            .ToDictionaryAsync(x => x.Key);

        logger.LogDebug("Sending birthday greeting messages in {GuildCount} guilds.", guilds.Count);

        await Parallel.ForEachAsync(guilds, async (pair, token) =>
        {
            string message = string.Join('\n', pair.Value.Select(mapGreetingMessage));
            var channel = (ITextChannel) await client.GetChannelAsync(pair.Key.BirthdayChannelId!.Value);
            await channel.SendMessageAsync(message);
        });
    }

    private static string mapGreetingMessage(User user) => user.Birthdate switch
    {
        FullDate fullDate => $"Auguri a {user.ToMentionString()}, che oggi compie {fullDate.GetAge()} anni!",
        MonthDay => $"Auguri a {user.ToMentionString()}!",
        _ => throw new ArgumentOutOfRangeException(nameof(user),"User with an unexpected kind of birthdate.")
    };
}
