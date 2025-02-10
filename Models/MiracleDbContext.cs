using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management_System__Miracle_Shop_.Models
{
    public class MiracleDbContext : IdentityDbContext<NewUserClass>
    {
        public MiracleDbContext(DbContextOptions<MiracleDbContext> options)
            : base(options)
        {
        }

        // DbSet for Products
        public DbSet<Product> Products { get; set; }

        public DbSet<StockMovement> StockMovement { get; set; }
        // DbSet for Folders
        public DbSet<Folder> Folders { get; set; }

        // If you want to explicitly configure relationships or constraints
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Example: Configure a one-to-many relationship between NewUserClass and Product
            modelBuilder.Entity<Product>()
                .HasOne(p => p.User)  // A Product belongs to one User
                .WithMany(u => u.Products)  // A User can have many Products
                .HasForeignKey(p => p.UserID)  // Foreign Key in Product
                .OnDelete(DeleteBehavior.Cascade); // Define how deletion works

            // Example: Configure a one-to-many relationship between NewUserClass and Folder
            modelBuilder.Entity<Folder>()
                .HasOne(f => f.User)  // A Folder belongs to one User
                .WithMany(u => u.Folders)  // A User can have many Folders
                .HasForeignKey(f => f.UserID)  // Foreign Key in Folder
                .OnDelete(DeleteBehavior.Cascade); // Define how deletion works
        }
    }
}
