// Copyright © Korshun Ivan. All rights reserved.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Library.DataSource.DB.Records;

namespace Template.Library.EntityFramework.EntityTypeConfiguration;

public class BookTypeConfiguration : IEntityTypeConfiguration<BookRecord>
{
    public void Configure(EntityTypeBuilder<BookRecord> builder)
    {
        builder.HasKey(b => b.SerialNumber);
        builder.Property(b => b.SerialNumber).UseIdentityColumn(); // Configures SQL Server IDENTITY(1,1)

        // Configure Guid alternate key
        builder.Property(b => b.Id).IsRequired();
        builder.HasIndex(b => b.Id).IsUnique();

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(b => b.PageCount)
            .HasColumnType("smallint"); // SQL Server: from -32,768 to 32,767

        // Configure Genre Enum (Stored as string in DB for readability)
        builder.Property(b => b.Genre)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.Property(b => b.Available)
            .HasDefaultValue(false);

        builder.Property(b => b.AddedAt)
            .HasPrecision(0);

        builder.Ignore(b => b.Authors);

        // Optimistic concurency
        builder.Property(b => b.RowVersion)
            .IsRequired()
            .HasColumnType("rowversion")
            .IsRowVersion();

        builder.HasIndex(b => b.Title);
    }
}
