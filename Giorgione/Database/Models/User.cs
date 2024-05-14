// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel.DataAnnotations;

namespace Giorgione.Database.Models;

public sealed class User(ulong id)
{
    [Key]
    public ulong Id { get; init; } = id;

    public DateOnly? Birthday { get; set; }
}
