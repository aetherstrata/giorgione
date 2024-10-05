// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Giorgione.Database.Models;

using Microsoft.EntityFrameworkCore;

namespace Giorgione.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasKey(u => u.Id);

        modelBuilder.Entity<User>().Property(u => u.BirthdayRepresentation)
            .HasColumnName("Birthday")
            .HasColumnType("date")
            .IsRequired(false);

        modelBuilder.Entity<User>().Ignore(u => u.Birthdate);
    }
}
