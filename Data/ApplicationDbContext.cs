using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FPTBook.Models;

namespace FPTBook.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<FPTBook.Models.Category> Category { get; set; }
        public DbSet<FPTBook.Models.Book> Book { get; set; }
        public DbSet<FPTBook.Models.Order> Order { get; set; }
    }
}