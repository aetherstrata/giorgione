// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Giorgione.Config;
using Giorgione.Models;

using Microsoft.EntityFrameworkCore;

namespace Giorgione;

internal class DatabaseContext : DbContext
{
    public DbSet<User> Users { get; set; }
}
