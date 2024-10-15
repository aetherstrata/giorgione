// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq.Expressions;

using Giorgione.Data.Models;

namespace Giorgione.Data.Filters;

/// <summary>
/// This class provides a collection of filter expressions to use in `where` clauses
/// </summary>
/// <remarks>
/// This is useful when there is a need to apply a predicate on unexposed representation fields
/// </remarks>
public static class UserFilter
{
    /// <summary>
    /// Filter only users that have a birthdate on a specified date
    /// </summary>
    /// <param name="date">The date to filter by</param>
    /// <returns>the built filter expression</returns>
    public static Expression<Func<User, bool>> ByBirthdate(DateTime date)
    {
        return user => user.BirthdayRepresentation != null &&
                       user.BirthdayRepresentation.Value.Day == date.Day &&
                       user.BirthdayRepresentation.Value.Month == date.Month;
    }

    /// <summary>
    /// Filter only users that have a birthdate set
    /// </summary>
    public static readonly Expression<Func<User, bool>> HasBirthdate = user => user.BirthdayRepresentation != null;
}
