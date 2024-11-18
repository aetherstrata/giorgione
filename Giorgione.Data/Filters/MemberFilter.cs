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
    /// <param name="query">The database query</param>
    /// <returns>The filtered query</returns>
    public static IQueryable<Member> HasBirthdate(this IQueryable<Member> query)
    {
        return query
            .Where(m => m.DisplayBirthday)
            .Where(m => m.Guild.BirthdayChannelId.HasValue)
            .Where(m => m.User.BirthdayRepresentation.HasValue);
    }

    /// <summary>
    /// Filter only members that have a birthdate
    /// </summary>
    /// <param name="query">The database query</param>
    /// <param name="date">The date to filter by</param>
    /// <returns>The filtered query</returns>
    public static IQueryable<Member> WithBirthdate(this DbSet<Member> query, DateTime date)
    {
        return query
            .Where(m => m.DisplayBirthday)
            .Where(m => m.Guild.BirthdayChannelId.HasValue)
            .Where(m => m.User.BirthdayRepresentation.HasValue &&
                        m.User.BirthdayRepresentation.Value.Month == date.Month &&
                        m.User.BirthdayRepresentation.Value.Day == date.Day);
    }
}
