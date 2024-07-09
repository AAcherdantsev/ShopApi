using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ShopApi.Models.Base;
using ShopApi.Models.Enums;

namespace ShopApi.Models.Orders;

[Index(nameof(OrderNumber), IsUnique = true)]
public class Order : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string OrderNumber { get; set; }

    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; }

    [Required]
    public OrderStatus Status { get; set; }

    [Required]
    public DateTime Created { get; set; }

    public IList<OrderItem> Items { get; set; }

    public override string ToString()
    {
        return $"Number:{OrderNumber}, Customer:{CustomerName}, " +
               $"Status:{Status}, Created:{Created:dd.MM.yyyy hh:mm:ss}";
    }
}