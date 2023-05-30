using Microsoft.EntityFrameworkCore;
using ProcessingService.Entities;
using System.Net.Mail;
using System.Net.Sockets;

namespace ProcessingService
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options)
        {
            this.Database.EnsureCreated();
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Identifier> Identifiers { get; set; }
        public DbSet<LinkPrice> LinkPrices { get; set; }


       /* protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>(entity =>
            {
                entity.Property(e => e.TimeRetrieved).HasColumnType("datetime");

                entity.Property(e => e.PublishedDate).HasColumnType("datetime");

                *//*entity.HasMany(d => d.Authors)
                    .WithMany(p => p.Books)
                    .UsingEntity<Dictionary<string, object>>(
                        "AuthorBook",
                        l => l.HasOne<Author>().WithMany().HasForeignKey("AuthorId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_13"),
                        r => r.HasOne<Book>().WithMany().HasForeignKey("BookId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_12"),
                        j =>
                        {
                            j.HasKey("AuthorId", "BookId");

                            j.ToTable("AuthorBook");
                        });*//*

                entity.HasMany(d => d.Categories)
                    .WithMany(p => p.Books)
                    .UsingEntity<Dictionary<string, object>>(
                        "BookCategory",
                        l => l.HasOne<Category>().WithMany().HasForeignKey("CategoryId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_15"),
                        r => r.HasOne<Book>().WithMany().HasForeignKey("BookId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_14"),
                        j =>
                        {
                            j.HasKey("BookId", "CategoryId");

                            j.ToTable("BookCategory");
                        });

                *//*entity.HasMany(d => d.IndustryIdentifiers)
                    .WithOne(p => p.Books)
                    .HasForeignKey(d => d.IdentifierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_16");

                entity.HasMany(d => d.Origin)
                    .WithOne(p => p.Books)
                    .HasForeignKey(d => d.LinkPriceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_17");*//*

            });

        }*/
    }
}
