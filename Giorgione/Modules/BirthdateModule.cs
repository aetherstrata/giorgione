// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Discord.Interactions;
using Discord;

using Giorgione.Data;
using Giorgione.Data.Extensions;
using Giorgione.Data.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Giorgione.Modules;

[Group("birthday", "Set your birthday and check upcoming ones")]
public class BirthdateModule(AppDbContext db, ILogger<BirthdateModule> logger) : BotModule(logger)
{
    [SlashCommand("show", "Show your birthdate")]
    public async Task GetBirthdayAsync(IUser? user = null)
    {
        ulong id = user?.Id ?? Context.User.Id;

        try
        {
            var userEntity = await db.Users.FindAsync(id);
            string message = userEntity is null
                ? "User was not found in the database"
                : userEntity.Birthdate.ToShortString();

            await RespondAsync(message);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Could not retrieve birthday data for {UserId}", id);
            await RespondAsync("An error occurred.");
        }
    }

    [SlashCommand("set", "Set your birthday")]
    public async Task BirthdaySetAsync(string birthday)
    {
        var birthdate = Birthdate.Parse(birthday);

        try
        {
            var user = await db.Users.FindAsync(Context.User.Id);

            if (user is not null)
            {
                user.Birthdate = birthdate;
                db.Users.Update(user);
            }
            else
            {
                user = new User(Context.User.Id)
                {
                    Birthdate = birthdate
                };
                db.Users.Add(user);
            }

            await db.SaveChangesAsync();

            var embed = new EmbedBuilder()
                .WithColor(Color.Teal)
                .WithDescription($"Your birthday has been set to {birthdate}")
                .Build();

            Logger.LogDebug("{UserId} has set its birthday to {Birthdate}", Context.User.Id, birthdate);

            await RespondAsync(embed: embed);
        }
        catch (FormatException e)
        {
            Logger.LogError(e, "An error occurred while processing a '/birthday set' command");

            var formatEmbed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("Format error")
                .WithDescription("""
                                 The birthdate does not have a valid format.
                                 Valid formats are:
                                 - `YYYY-M-D` | `D-M` | `D-M-YYYY`
                                 - `YYYY/M/D` | `D/M` | `D/M/YYYY`
                                 """)
                .Build();

            await RespondAsync(embed: formatEmbed);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An error occurred while processing a '/birthday set' command");
            await RespondAsync("An error occurred", ephemeral: true);
        }
    }

    [SlashCommand("list", "List all birthday defined in this server")]
    public async Task BirthdayListAsync()
    {
        try
        {

            var list = await db.Users
                .WithBirthday()
                .Select(static user => $"<@{user.Id}> - {user.Birthdate.ToShortString()}")
                .ToListAsync();

            var listEmbed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithTitle("Lista dei festeggiati")
                .WithDescription(string.Join('\n', list))
                .Build();

            await RespondAsync(embed: listEmbed);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An error occurred while processing a '/birthday list' command");
            await RespondAsync("An error occurred", ephemeral: true);
        }
    }
}
