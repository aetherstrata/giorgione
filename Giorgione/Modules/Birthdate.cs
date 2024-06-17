// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;

using Discord.Interactions;
using Discord;

using Giorgione.Database;
using Giorgione.Database.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Giorgione.Modules;

[Group("birthday", "Set your birthday and check upcoming ones")]
public class Birthdate(
    IDbContextFactory<UsersDbContext> dbFactory,
    ILogger<Birthdate> logger) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("show", "Show your birthdate")]
    public Task GetBirthdayAsync(IUser? userParam = null)
    {
        try
        {
            User? user;

            using (var db = dbFactory.CreateDbContext())
            {
                user = db.Users.Find(userParam?.Id ?? Context.User.Id);
            }

            return user is null
                ? RespondAsync($"Not set")
                : RespondAsync($"{user.Birthday}");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Could not retrieve birthday data for {UserId}", Context.User.Id);
            return RespondAsync("Errore");
        }
    }

    [SlashCommand("set", "Set your birthday")]
    public Task BirthdaySetAsync(string birthday)
    {
        try
        {
            User? user;

            var date = DateOnly.ParseExact(birthday, ["yyyy-M-d", "M-d"], CultureInfo.InvariantCulture);

            using (var db = dbFactory.CreateDbContext())
            {
                user = db.Users.Find(Context.User.Id);

                if (user is null)
                {
                    user = new User(Context.User.Id)
                    {
                        Birthday = date
                    };
                    db.Add(user);
                }
                else
                {
                    user.Birthday = date;
                    db.Update(user);
                }

                db.SaveChanges();
            }

            var embed = new EmbedBuilder()
                .WithDescription($"{user.Birthday}")
                .Build();

            return RespondAsync(embed: embed);
        }
        catch (FormatException e)
        {
            logger.LogError(e, "An error occurred while processing a '/birthday set' command");

            var formatEmbed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("Format error")
                .WithDescription("The birthdate does not have a valid format.\nValid formats are: `YYYY-M-D` and `M-D`")
                .Build();

            return RespondAsync(embed: formatEmbed);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while processing a '/birthday set' command");
            return RespondAsync("An error occurred", ephemeral: true);
        }
    }

    [SlashCommand("list", "List all birthday defined in this server")]
    public async Task BirthdayListAsync()
    {
        try
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var list = await db.Users
                    .Where(user => user.Birthday.HasValue)
                    .Select(user => $"{user.Id} - {user.Birthday}")
                    .ToListAsync();

                var listEmbed = new EmbedBuilder()
                     .WithColor(Color.Blue)
                     .WithTitle("Lista dei festeggiati")
                     .WithDescription(string.Join('\n',list))
                     .Build();

                await RespondAsync(embed: listEmbed);              
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while processing a '/birthday list' command");
            await RespondAsync("An error occurred", ephemeral: true);           
        }
    }
}
