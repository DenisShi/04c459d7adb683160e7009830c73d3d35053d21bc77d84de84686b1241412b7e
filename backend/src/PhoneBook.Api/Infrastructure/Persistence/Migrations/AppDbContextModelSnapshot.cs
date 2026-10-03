using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PhoneBook.Api.Infrastructure.Persistence;

#nullable disable

namespace PhoneBook.Api.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PhoneBook.Api.Domain.PhoneNumber", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("ContactName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("contact_name");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Number")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("character varying(16)")
                        .HasColumnName("number");

                    b.Property<string>("OwnerId")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("owner_id");

                    b.Property<string>("OwnerUsername")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("owner_username");

                    b.Property<string>("Visibility")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("character varying(16)")
                        .HasColumnName("visibility");

                    b.HasKey("Id")
                        .HasName("pk_phone_numbers");

                    b.HasIndex("CreatedAt")
                        .IsDescending()
                        .HasDatabaseName("ix_phone_numbers_shared_created_at")
                        .HasFilter("visibility = 'SHARED'");

                    b.HasIndex("OwnerId", "CreatedAt")
                        .IsDescending(false, true)
                        .HasDatabaseName("ix_phone_numbers_owner_id_created_at");

                    b.ToTable("phone_numbers", null, t =>
                        {
                            t.HasCheckConstraint("ck_phone_numbers_number_format", "number ~ '^\\+?[0-9]{3,15}$'");

                            t.HasCheckConstraint("ck_phone_numbers_visibility", "visibility IN ('PERSONAL', 'SHARED')");
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
