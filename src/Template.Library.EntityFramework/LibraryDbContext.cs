// Copyright © Korshun Ivan. All rights reserved.

using Microsoft.EntityFrameworkCore;
using Template.Library.DataSource.DB.Records;
using Template.Library.EntityFramework.EntityTypeConfiguration;

namespace Template.Library.EntityFramework;

public class LibraryDbContext(DbContextOptions<LibraryDbContext> dbContextOptions) : DbContext(dbContextOptions)
{
    public virtual DbSet<BookRecord> Books => Set<BookRecord>();

    public virtual DbSet<AuthorRecord> Authors => Set<AuthorRecord>();

    public virtual DbSet<BookAuthorRecord> BookAuthors => Set<BookAuthorRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Option A: Apply this specific configuration class explicitly
        modelBuilder.ApplyConfiguration(new BookTypeConfiguration());
        modelBuilder.ApplyConfiguration(new AuthorTypeConfiguration());
        modelBuilder.ApplyConfiguration(new BookAuthorTypeConfiguration());

        // Option B: Automatically find and apply ALL configurations in this assembly
        //modelBuilder.ApplyConfigurationsFromAssembly(typeof(LibraryDbContext).Assembly);
    }
}
