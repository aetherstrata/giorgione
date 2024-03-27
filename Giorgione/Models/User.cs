// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel.DataAnnotations;

namespace Giorgione.Models;

internal sealed class User
{
    [Key]
    public ulong Id { get; set; }

    public DateOnly? Birthday { get; set; }
}
