// Copyright © Korshun Ivan. All rights reserved.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Library.DataSource.DB.Records;

namespace Template.Library.EntityFramework.EntityTypeConfiguration;

public class BookAuthorTypeConfiguration : IEntityTypeConfiguration<BookAuthorRecord>
{
    public void Configure(EntityTypeBuilder<BookAuthorRecord> builder)
    {
        builder.HasKey(ba => new { ba.BookSn, ba.AuthorSn });

        builder.HasOne<BookRecord>()
               .WithMany()
               .HasForeignKey(ba => ba.BookSn)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AuthorRecord>()
               .WithMany()
               .HasForeignKey(ba => ba.AuthorSn)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
