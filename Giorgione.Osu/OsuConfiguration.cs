// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Giorgione.Osu;

public sealed class OsuConfiguration
{
    public required int ClientId { get; init; }
    public required string ClientSecret { get; init; }
}
