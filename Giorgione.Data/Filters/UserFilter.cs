// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq.Expressions;

using Giorgione.Data.Models;

using Microsoft.EntityFrameworkCore;

namespace Giorgione.Data.Filters;

/// <summary>
/// This class provides a collection of filters for <see cref="User"/> queries
/// </summary>
/// <remarks>
/// This is useful when there is a need to apply a predicate on unexposed representation fields
/// </remarks>
public static class UserFilter
{
    /// <summary>
    /// Filter only users that have a birthdate on a specified date
    /// </summary>
    /// <param name="query">The database query</param>
    /// <param name="date">The date to filter by</param>
    /// <returns>The filtered query</returns>
    public static IQueryable<User> FindByBirthdate(this IQueryable<User> query, DateTime date)
    {
        return query.Where(user => user.BirthdayRepresentation.HasValue &&
                           user.BirthdayRepresentation.Value.Day == date.Day &&
                           user.BirthdayRepresentation.Value.Month == date.Month);
    }

    /// <summary>
    /// Filter only users that have a set birthdate
    /// </summary>
    public static IQueryable<User> HasBirthdate(this IQueryable<User> query)
    {
        return query.Where(user => user.BirthdayRepresentation.HasValue);
    }
}
