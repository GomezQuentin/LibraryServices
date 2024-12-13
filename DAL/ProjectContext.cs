using Microsoft.EntityFrameworkCore;
using DAL.Models;

namespace DAL
{
    public class ProjectContext : DbContext
    {
        public ProjectContext(DbContextOptions<ProjectContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<LibraryTransaction> LibraryTransactions { get; set; }
    }
}
