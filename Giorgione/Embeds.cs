// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Discord;

namespace Giorgione;

public static class Embeds
{
    public static Embed GenericError(string title, string message)
    {
        var embed = new EmbedBuilder()
            .WithTitle($"\u26a0\ufe0f {title}")
            .WithDescription(message)
            .WithColor(Color.Red)
            .Build();

        return embed;
    }
}
