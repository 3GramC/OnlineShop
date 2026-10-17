using Microsoft.EntityFrameworkCore;
using OnlineShop.Api.Domain.Entities;

namespace OnlineShop.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Gaming" },
                new Category { Id = 2, Name = "Office" }
                );
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "ASUS ROG Strix G15", Description = "Gaming laptop", Price = 50000000m, StockQuantity = 5, CategoryId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Product { Id = 2, Name = "Lenovo ThinkPad X1", Description = "Business laptop", Price = 45000000m, StockQuantity = 3, CategoryId = 2, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
                );
        }
    }

}
