// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Discord.Interactions;
using Discord;

namespace Giorgione.Modules;

public class Birthdate : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("birthday", "Auguri di compleanno")]
    public Task BirthDayAsync()
    {
        var embed = new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithAuthor(a => a.WithName("\uD83C\uDF82 Buon Compleanno!"))
            .WithDescription(
                $"""
                 Sono Davvero Contento che oggi sia il compleanno di <@{Context.Guild.Id}>
                 """)
            .Build();

        return RespondAsync(embed: embed);
    }
}
