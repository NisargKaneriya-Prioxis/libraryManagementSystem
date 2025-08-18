using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EA.Model.Models.MyLibraryDB;

public partial class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__Book__3DE0C2273540C47C");

            entity.Property(e => e.BorrowedStatus).HasDefaultValue(1);
            entity.Property(e => e.Status).HasDefaultValue(4);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
