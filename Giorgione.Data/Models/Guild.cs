// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Giorgione.Data.Models;

public sealed class Guild(ulong id) : IHasPrimaryKey<Guild, ulong>
{
    public ulong Id { get; } = id;

    public ulong? BirthdayChannelId { get; set; }

    public ulong? StarboardId { get; set; }

    public ulong? AnimeFeedChannelId { get; set; }

    /// <inheritdoc />
    public static Guild Create(ulong id) => new(id);
}
