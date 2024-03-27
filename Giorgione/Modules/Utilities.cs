// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Discord;
using Discord.Interactions;

namespace Giorgione.Modules;

public class Utilities : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Controlla se Giorgione è in casa")]
    public Task PingAsync()
    {
        var embed = new EmbedBuilder()
            .WithColor(Color.Gold)
            .WithAuthor(a => a.WithName("\ud83c\udfd3 Pong!"))
            .WithDescription(
                $"""
                 Battuta di risposta con ritardo di **{DateTimeOffset.UtcNow.Subtract(Context.Interaction.CreatedAt).Milliseconds}ms**
                 Camminata ai server di Discord in **{Context.Client.Latency}ms**
                 """)
            .Build();

        return RespondAsync(embed: embed);
    }

    [SlashCommand("server", "Controlla le informazioni del server")]
    public async Task ServerInfoAsync()
    {
        await Context.Guild.DownloadUsersAsync();

        var embed = new EmbedBuilder()
            .WithAuthor(a => a.WithName(Context.Guild.Name))
            .WithThumbnailUrl(Context.Guild.IconUrl)
            .WithFields(
                new EmbedFieldBuilder()
                    .WithName("Owner")
                    .WithValue($"<@{Context.Guild.Owner.Id}>"),
                new EmbedFieldBuilder()
                    .WithName("Members")
                    .WithValue(Context.Guild.MemberCount.ToString())
                    .WithIsInline(true),
                new EmbedFieldBuilder()
                    .WithName("Online")
                    .WithValue(Context.Guild.Users
                        .Count(u => !u.IsBot && u.Status != UserStatus.Offline && u.Status != UserStatus.Invisible)
                        .ToString())
                    .WithIsInline(true),
                new EmbedFieldBuilder()
                    .WithName("Features")
                    .WithValue(Context.Guild.Features.Value),
                new EmbedFieldBuilder()
                    .WithName("Experimental features")
                    .WithValue(string.Join(", ", Context.Guild.Features.Experimental)))
            .Build();

        await RespondAsync(embed: embed);
    }
}
