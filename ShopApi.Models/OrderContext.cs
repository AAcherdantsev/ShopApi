using Microsoft.EntityFrameworkCore;
using ShopApi.Models.Orders;

namespace ShopApi.Models;

public class OrderContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    public OrderContext(DbContextOptions<OrderContext> options)
    : base(options) { }

    public OrderContext() { }
}