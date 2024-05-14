// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Giorgione.Config;

public sealed class BotConfig
{
    public required string Token { get; init; }
    public required ulong GuildId { get; init; }
    public required ulong StarboardId { get; init; }
}
