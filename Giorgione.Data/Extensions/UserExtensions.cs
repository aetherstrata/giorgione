// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Giorgione.Data.Models;

namespace Giorgione.Data.Extensions;

public static class UserExtensions
{
    public static IQueryable<User> CelebratingBirthday(this IQueryable<User> query, DateTime date) => query
        .Where(user => user.BirthdayRepresentation != null &&
                       user.BirthdayRepresentation.Value.Day == date.Day &&
                       user.BirthdayRepresentation.Value.Month == date.Month);

    public static IQueryable<User> CelebratingBirthday(this IQueryable<User> query) => query
        .Where(user => user.BirthdayRepresentation != null &&
                       user.BirthdayRepresentation.Value.Day == DateTime.Now.Day &&
                       user.BirthdayRepresentation.Value.Month == DateTime.Now.Month);

    public static IQueryable<User> WithBirthday(this IQueryable<User> query) => query
        .Where(user => user.BirthdayRepresentation != null);
}
