// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Giorgione.Data.Models;

/// <summary>
/// Bot user model
/// </summary>
/// <param name="id">The Discord ID of the user</param>
public sealed class User(ulong id)
{
    /// The Discord ID of the user
    public ulong Id { get; } = id;

    /// <summary>
    /// The user birthdate
    /// </summary>
    /// <remarks>To see how this property is persisted, see <see cref="BirthdayRepresentation"/></remarks>
    public Birthdate Birthdate
    {
        get => BirthdayRepresentation.ToBirthdate();
        set => BirthdayRepresentation = value.ToRepresentation();
    }

    public string ToMentionString() => $"<@{Id}>";

    internal DateOnly? BirthdayRepresentation { get; private set; }
}
