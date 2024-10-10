// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;

namespace Giorgione.Data.Models;

/// Base model for birthdates
public abstract record Birthdate
{
    private static readonly string[] full_formats = ["yyyy-M-d", "yyyy/M/d", "d-M-yyyy", "d/M/yyyy"];
    private static readonly string[] mday_formats = ["d-M", "d/M"];

    public static Birthdate Parse(string message)
    {
        if (message.Length <= 5)
        {
            var date = DateOnly.ParseExact(message, mday_formats, CultureInfo.InvariantCulture);

            return new MonthDay(date.Month, date.Day);
        }
        else
        {
            var date = DateOnly.ParseExact(message, full_formats, CultureInfo.InvariantCulture);

            return new FullDate(date);
        }
    }

    /// <summary>
    /// Human-readable representation of this birthdate.
    /// </summary>
    /// <returns>the formatted date</returns>
    public abstract string ToShortString();
}

/// <summary>
/// Complete birthday date
/// </summary>
/// <remarks>Includes year, month and day of birth</remarks>
/// <param name="Date">The birthday</param>
public sealed record FullDate(DateOnly Date) : Birthdate
{
    public override string ToShortString() => Date.ToString("dd/MM/yyyy");
}

/// <summary>
/// Month and day of birth
/// </summary>
/// <param name="Month">The month of birth</param>
/// <param name="Day">The day of birth</param>
public sealed record MonthDay(int Month, int Day) : Birthdate
{
    /// <inheritdoc />
    public override string ToShortString() => $"{Month:D2}/{Day:D2}";
}

/// <summary>
/// The birthdate is not set
/// </summary>
public sealed record NotSet : Birthdate
{
    /// <inheritdoc />
    public override string ToShortString() => "Not set";
}

public static class BirthdateExtensions
{
    public static Birthdate ToBirthdate(this DateOnly? date) => date switch
    {
        null => new NotSet(),
        { Year: < 1900 } => new MonthDay(date.Value.Month, date.Value.Day),
        _ => new FullDate(date.Value)
    };

    internal static DateOnly? ToRepresentation(this Birthdate birthdate) => birthdate switch
    {
        NotSet => null,
        FullDate fullDate => fullDate.Date,
        MonthDay monthDay => new DateOnly(1, monthDay.Month, monthDay.Day),
        _ => throw new ArgumentException($"Unknown birthdate kind: {birthdate}")
    };

    public static TResult Map<TResult>(this Birthdate birthdate,
        Func<FullDate, TResult> mapFull, Func<MonthDay, TResult> mapMonthDay, Func<TResult> mapNotSet) => birthdate switch
    {
        NotSet => mapNotSet(),
        FullDate fullDate => mapFull(fullDate),
        MonthDay monthDay => mapMonthDay(monthDay),
        _ => throw new ArgumentException($"Unknown birthdate kind: {birthdate}")
    };
}
