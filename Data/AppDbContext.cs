using Microsoft.EntityFrameworkCore;
using FreedomITAS.Models;

namespace FreedomITAS.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ClientModel> Clients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClientModel>().HasKey(c => c.ClientId); // Primary key: ClientId

            modelBuilder.Entity<ClientModel>().Property(c => c.Id).ValueGeneratedOnAdd(); // Auto-incremented secondary ID
        }
    }
}
