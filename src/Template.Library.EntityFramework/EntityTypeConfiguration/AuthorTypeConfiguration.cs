// Copyright © Korshun Ivan. All rights reserved.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Library.DataSource.DB.Records;

namespace Template.Library.EntityFramework.EntityTypeConfiguration;

public class AuthorTypeConfiguration : IEntityTypeConfiguration<AuthorRecord>
{
    public void Configure(EntityTypeBuilder<AuthorRecord> builder)
    {
        builder.HasKey(a => a.SerialNumber);
        builder.Property(a => a.SerialNumber).UseIdentityColumn(); // Configures SQL Server IDENTITY(1,1)

        // Configure Guid alternate key
        builder.Property(a => a.Id).IsRequired();
        builder.HasIndex(a => a.Id).IsUnique();

        builder.Property(a => a.FullName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.Country)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.Birthday)
            .HasPrecision(0);

        builder.Property(b => b.Famous)
            .HasDefaultValue(false);

        // Optimistic concurency
        builder.Property(a => a.RowVersion)
            .IsRequired()
            .HasColumnType("rowversion")
            .IsRowVersion();

        builder.HasIndex(a => new { a.FullName }).IsUnique();
    }
}
