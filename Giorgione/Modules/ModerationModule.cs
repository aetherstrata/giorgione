// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Discord;
using Discord.Interactions;

using Microsoft.Extensions.Logging;

namespace Giorgione.Modules;

public class ModerationModule(ILogger<ModerationModule> logger) : BotModule(logger)
{
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    [SlashCommand("ban", "Ban a user")]
    public async Task BanAsync(IUser user,
        [Summary("reason", "The reason for this ban")]
        string? reason = null,
        [Summary("delete_messages", "How many days of their recent history to delete (Max 7 days)")]
        int pruneDays = 0)
    {
        try
        {
            await Context.Guild.AddBanAsync(user, pruneDays, reason);
        }
        catch (ArgumentException ex)
        {
            await RespondError("Unmet preconditions", ex.Message);
        }
        catch (Exception ex)
        {
            await RespondError("Error", ex.Message);
        }

        var embed = new EmbedBuilder()
            .WithTitle("Member Banned")
            .WithDescription($"<@{user.Id}> was banned by <@{Context.User.Id}>")
            .WithColor(Color.Orange)
            .WithThumbnailUrl(user.GetDisplayAvatarUrl())
            .WithCurrentTimestamp();

        if (reason is not null)
        {
            embed.AddField("Reason", reason);
        }

        await RespondAsync(embed: embed.Build());
    }

    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    [SlashCommand("softban", "Ban and immediately unban a user")]
    public async Task SoftbanAsync(IUser user,
        [Summary("reason", "The reason for this ban")]
        string? reason = null,
        [Summary("delete_messages", "How many days of their recent history to delete (Max 7 days)")]
        int pruneDays = 0)
    {
        try
        {
            await Context.Guild.AddBanAsync(user, pruneDays, reason);
            await Context.Guild.RemoveBanAsync(user);
        }
        catch (ArgumentException ex)
        {
            await RespondError("Unmet preconditions", ex.Message);
        }
        catch (Exception ex)
        {
            await RespondError("Error", ex.Message);
        }

        var embed = new EmbedBuilder()
            .WithTitle("Member Softbanned")
            .WithDescription($"<@{user.Id}> was softbanned by <@{Context.User.Id}>")
            .WithColor(Color.Orange)
            .WithThumbnailUrl(user.GetDisplayAvatarUrl())
            .WithCurrentTimestamp();

        if (reason is not null)
        {
            embed.AddField("Reason", reason);
        }

        await RespondAsync(embed: embed.Build());
    }

    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    [SlashCommand("unban", "Unban a user")]
    public async Task UnbanAsync(IUser user)
    {
        try
        {
            await Context.Guild.RemoveBanAsync(user);
        }
        catch (ArgumentException ex)
        {
            await RespondError("Unmet preconditions", ex.Message);
        }
        catch (Exception ex)
        {
            await RespondError("Error", ex.Message);
        }

        var embed = new EmbedBuilder()
            .WithTitle("Member Unbanned")
            .WithDescription($"<@{user.Id}> was unbanned by <@{Context.User.Id}>")
            .WithColor(Color.Orange)
            .WithThumbnailUrl(user.GetDisplayAvatarUrl())
            .WithCurrentTimestamp()
            .Build();

        await RespondAsync(embed: embed);
    }

    [RequireUserPermission(GuildPermission.KickMembers)]
    [RequireBotPermission(GuildPermission.KickMembers)]
    [SlashCommand("kick", "Kick a user")]
    public async Task KickAsync(IGuildUser user,
        [Summary("reason", "The reason for this kick")]
        string? reason = null)
    {
        try
        {
            await user.KickAsync(reason);
        }
        catch (ArgumentException ex)
        {
            await RespondError("Unmet preconditions", ex.Message);
        }
        catch (Exception ex)
        {
            await RespondError("Error", ex.Message);
        }

        var embed = new EmbedBuilder()
            .WithTitle("Member Kicked")
            .WithDescription($"<@{user.Id}> was kicked by <@{Context.User.Id}>")
            .WithColor(Color.Orange);

        if (reason is not null)
        {
            embed.AddField("Reason", reason);
        }

        await RespondAsync(embed: embed.Build());
    }
}
