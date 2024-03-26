// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Giorgione.Models;

using Microsoft.EntityFrameworkCore;

namespace Giorgione;

internal class Database 
{
    public DbSet<User> Users { get; set; }
}
