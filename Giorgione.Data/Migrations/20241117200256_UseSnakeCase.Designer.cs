﻿// <auto-generated />
using System;
using Giorgione.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Giorgione.Database.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20241117200256_UseSnakeCase")]
    partial class UseSnakeCase
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Giorgione.Data.Models.Guild", b =>
                {
                    b.Property<decimal>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<long?>("starboard_id")
                        .HasColumnType("bigint");

                    b.HasKey("id");

                    b.ToTable("guilds");
                });

            modelBuilder.Entity("Giorgione.Data.Models.User", b =>
                {
                    b.Property<decimal>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateOnly?>("BirthdayRepresentation")
                        .HasColumnType("date")
                        .HasColumnName("birthday");

                    b.HasKey("id");

                    b.ToTable("users");
                });
#pragma warning restore 612, 618
        }
    }
}