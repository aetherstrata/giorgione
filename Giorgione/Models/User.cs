// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giorgione.Models;
internal class User
{
    [Key]
    public ulong UserId { get; set; }

    public DateOnly Birthday { get; set; }
}
