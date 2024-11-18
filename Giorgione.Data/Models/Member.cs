// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq.Expressions;

namespace Giorgione.Data.Models;

/// <summary>
/// Join table between <see cref="Guild"/> and <see cref="User"/> with per-guild settings of the user
/// </summary>
public class Member(ulong guildId, ulong userId) : IHasPrimaryKey<Member, (ulong GuildId, ulong UserId)>
{
    public ulong GuildId { get; set; } = guildId;
    public ulong UserId { get; set; } = userId;

    public Guild Guild { get; set; }
    public User User { get; set; }

    public bool DisplayBirthday { get; set; } = false;

    /// <inheritdoc />
    public static Expression<Func<Member, bool>> BuildFindExpression((ulong GuildId, ulong UserId) id)
    {
        return user => user.GuildId == id.GuildId &&
                       user.UserId == id.UserId;
    }

    /// <inheritdoc />
    public static Member Create((ulong GuildId, ulong UserId) id) => new(id.GuildId, id.UserId);
}
