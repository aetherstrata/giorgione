// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Discord.Interactions;
using Discord;

using Giorgione.Data;
using Giorgione.Data.Extensions;
using Giorgione.Data.Filters;
using Giorgione.Data.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Giorgione.Modules;

[Group("birthday", "Set your birthday and check upcoming ones")]
public class BirthdateModule(AppDbContext db, ILogger<BirthdateModule> logger) : BotModule(logger)
{
    [Group("alert", "Manage the channel where birthday alerts are sent")]
    public class Alert(AppDbContext db, ILogger<BirthdateModule> logger) : BotModule(logger)
    {
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [SlashCommand("enable", "Enable birthday alerts in the selected channel")]
        public async Task EnableChannelAsync(ITextChannel channel)
        {
            try
            {
                await db.Guilds.UpsertAsync(Context.Guild.Id, g => g.BirthdayChannelId = channel.Id);
                await RespondAsync("The birthday channel has been enabled.");
            }
            catch (Exception ex)
            {
                await RespondError(ex, "Error","Could not upsert birthday channel. See logs for more information.");
            }
        }

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [SlashCommand("disable", "Disable birthday alerts")]
        public async Task DisableChannelAsync()
        {
            try
            {
                await db.Guilds.UpsertAsync(Context.Guild.Id, g => g.BirthdayChannelId = null);
                await RespondAsync("The birthday channel has been disabled.");
            }
            catch (Exception ex)
            {
                await RespondError(ex, "Error","Could not upsert birthday channel. See logs for more information.");
            }
        }
    }

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
            await db.Users.UpsertAsync(Context.User.Id, u => u.Birthdate = birthdate);

            await db.Members.UpsertAsync((Context.Guild.Id, Context.User.Id), u => u.DisplayBirthday = true);

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
            await RespondError(e, "Error", "An error occurred. See log for further details.");
        }
    }

    [SlashCommand("display", "Display your birthday on this server")]
    public async Task BirthdayDisplayAsync()
    {
        try
        {
            await db.Members.UpsertAsync((Context.Guild.Id, Context.User.Id), u => u.DisplayBirthday = true);
        }
        catch (Exception e)
        {
            await RespondError(e, "Error", "An error occurred. See log for further details.");
        }
    }

    [SlashCommand("hide", "Hide your birthday from this server")]
    public async Task BirthdayHideAsync()
    {
        try
        {
            await db.Members.UpsertAsync((Context.Guild.Id, Context.User.Id), u => u.DisplayBirthday = false);
        }
        catch (Exception e)
        {
            await RespondError(e, "Error", "An error occurred. See log for further details.");
        }
    }

    [SlashCommand("remove", "Remove your birthday")]
    public async Task BirthdayRemoveAsync()
    {
        try
        {
            await db.Users.UpsertAsync(Context.User.Id, u => u.Birthdate = new NotSet());

            await db.Members
                .Where(gu => gu.UserId == Context.User.Id)
                .ExecuteUpdateAsync(calls => calls.SetProperty(x => x.DisplayBirthday, false));

            var embed = new EmbedBuilder()
                .WithColor(Color.Green)
                .WithTitle("Success")
                .WithDescription("Your birthday has been removed.")
                .Build();

            await RespondAsync(embed: embed);
        }
        catch (Exception e)
        {
            await RespondError(e, "Error", "An error occurred. See log for further details.");
        }
    }

    [SlashCommand("list", "List all birthday defined in this server")]
    public async Task BirthdayListAsync()
    {
        try
        {
            var list = await db.Members
                .Where(gu => gu.GuildId == Context.Guild.Id)
                .HasBirthdate()
                .Select(static m => $"<@{m.User.Id}> - {m.User.Birthdate.ToShortString()}")
                .ToListAsync();

            var listEmbed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithTitle("Birthdays list")
                .WithDescription(list.Count > 0 ? string.Join('\n', list) : "No birthday defined.")
                .Build();

            await RespondAsync(embed: listEmbed);
        }
        catch (Exception e)
        {
            await RespondError(e, "Error", "An error occurred. See log for further details.");
        }
    }
}
