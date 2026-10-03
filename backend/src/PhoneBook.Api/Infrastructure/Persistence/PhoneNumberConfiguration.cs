using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoneBook.Api.Domain;

namespace PhoneBook.Api.Infrastructure.Persistence;

public sealed class PhoneNumberConfiguration : IEntityTypeConfiguration<PhoneNumber>
{
    public void Configure(EntityTypeBuilder<PhoneNumber> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("phone_numbers", table =>
        {
            table.HasCheckConstraint("ck_phone_numbers_visibility", "visibility IN ('PERSONAL', 'SHARED')");
            table.HasCheckConstraint("ck_phone_numbers_number_format", "number ~ '^\\+?[0-9]{3,15}$'");
        });

        builder.HasKey(entry => entry.Id).HasName("pk_phone_numbers");
        builder.Property(entry => entry.Id).ValueGeneratedNever();

        builder.Property(entry => entry.ContactName).HasMaxLength(PhoneNumberFormat.MaxContactNameLength).IsRequired();
        builder.Property(entry => entry.Number).HasMaxLength(16).IsRequired();
        builder.Property(entry => entry.Visibility)
            .HasMaxLength(16)
            .HasConversion(
                visibility => JsonNamingPolicy.SnakeCaseUpper.ConvertName(visibility.ToString()),
                stored => Enum.Parse<Visibility>(stored, true))
            .IsRequired();
        builder.Property(entry => entry.OwnerId).HasMaxLength(255).IsRequired();
        builder.Property(entry => entry.OwnerUsername).HasMaxLength(255).IsRequired();
        builder.Property(entry => entry.CreatedAt).IsRequired();

        builder.HasIndex(entry => new { entry.OwnerId, entry.CreatedAt })
            .HasDatabaseName("ix_phone_numbers_owner_id_created_at")
            .IsDescending(false, true);

        builder.HasIndex(entry => entry.CreatedAt)
            .HasDatabaseName("ix_phone_numbers_shared_created_at")
            .IsDescending(true)
            .HasFilter("visibility = 'SHARED'");
    }
}
