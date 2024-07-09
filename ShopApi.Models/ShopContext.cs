using Microsoft.EntityFrameworkCore;
using ShopApi.Models.Orders;

namespace ShopApi.Models;

public class ShopContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    public ShopContext(DbContextOptions<ShopContext> options)
    : base(options) { }

    public ShopContext() { }
}