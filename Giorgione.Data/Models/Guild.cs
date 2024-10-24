// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Giorgione.Data.Models;

public sealed class Guild(ulong id)
{
    public ulong Id { get; } = id;

    public ulong? StarboardId { get; set; }
}
