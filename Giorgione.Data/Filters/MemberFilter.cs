// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Giorgione.Data.Models;

using Microsoft.EntityFrameworkCore;

namespace Giorgione.Data.Filters;

/// <summary>
/// This class provides a collection of filters for <see cref="Member"/> queries
/// </summary>
/// <remarks>
/// This is useful when there is a need to apply a predicate on unexposed representation fields
/// </remarks>
public static class MemberFilter
{
    /// <summary>
    /// Filter only members that have a birthdate
    /// </summary>
    /// <param name="set"></param>
    /// <returns></returns>
    public static IQueryable<Member> WithBirthdate(this DbSet<Member> set)
    {
        return set
            .Where(m => m.Guild.BirthdayChannelId.HasValue)
            .Where(m => m.User.BirthdayRepresentation.HasValue)
            .Where(m => m.DisplayBirthday);
    }
}
