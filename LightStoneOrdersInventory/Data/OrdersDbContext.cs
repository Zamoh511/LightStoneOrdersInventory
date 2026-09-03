using Microsoft.EntityFrameworkCore;
using LightStoneOrdersInventory.Models;

namespace LightStoneOrdersInventory.Data;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(b =>
        {
            b.HasIndex(p => p.Sku).IsUnique();
            b.Property(p => p.Price).HasPrecision(18,2);
        });

        modelBuilder.Entity<Order>(b =>
        {
            b.HasIndex(o => o.ExternalOrderId).IsUnique();
            b.Property(o => o.PlacedAt);
        });

        modelBuilder.Entity<OrderItem>(b =>
        {
            b.HasOne(oi => oi.Order).WithMany(o => o.Items).HasForeignKey(oi => oi.OrderId);
            b.HasOne(oi => oi.Product).WithMany().HasForeignKey(oi => oi.ProductId);
            b.Property(oi => oi.UnitPrice).HasPrecision(18,2);
        });
    }
}

public static class SeedData
{
    public static async Task EnsureSeedDataAsync(OrdersDbContext db)
    {
        if (await db.Products.AnyAsync()) return;

        db.Products.Add(new Product { Sku = "SKU-001", Name = "Wireless Mouse", Price = 24.99m, AvailableStock = 50 });
        db.Products.Add(new Product { Sku = "SKU-002", Name = "Mechanical Keyboard", Price = 79.99m, AvailableStock = 20 });
        await db.SaveChangesAsync();
    }
}
