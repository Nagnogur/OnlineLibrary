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
    }
}
