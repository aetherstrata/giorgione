// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Giorgione.Config;

public sealed class BotConfiguration
{
    public required string Token { get; init; }
    public required ulong TestGuildId { get; init; }
    public required ulong SuperuserId { get; init; }
}
