﻿// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
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
    public Task GetBirthdayAsync(IUser? user = null)
    {
        try
        {
            using var db = dbFactory.CreateDbContext();

            var userEntity = db.Users.Find(user?.Id ?? Context.User.Id);

            return RespondAsync(userEntity is null ? "Not set" : $"{userEntity.Birthday}");
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

            var date = DateOnly.ParseExact(birthday, ["yyyy-M-d", "M-d", "d-M-yyyy", "d/M", "yyyy/M/d", "d/M/yyyy"], CultureInfo.InvariantCulture);

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
                .WithColor(Color.Teal)
                .WithDescription($"Your birthday has been set to {user.Birthday.Value.Day}/{user.Birthday.Value.Month}")
                .Build();

            return RespondAsync(embed: embed);
        }
        catch (FormatException e)
        {
            logger.LogError(e, "An error occurred while processing a '/birthday set' command");

            var formatEmbed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("Format error")
                .WithDescription("The birthdate does not have a valid format.\nValid formats are:\n- `YYYY-M-D` | `M-D` | `D-M-YYYY`\n- `YYYY/M/D` | `M/D` | `D/M/YYYY`")
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
            await using var db = await dbFactory.CreateDbContextAsync();

            var list = await db.Users
                .Where(user => user.Birthday.HasValue)
                .Select(user => $"<@{user.Id}> - {user.Birthday}")
                .ToListAsync();

            var listEmbed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithTitle("Lista dei festeggiati")
                .WithDescription(string.Join('\n',list))
                .Build();

            await RespondAsync(embed: listEmbed);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while processing a '/birthday list' command");
            await RespondAsync("An error occurred", ephemeral: true);
        }
    }
}
