using System.ComponentModel.DataAnnotations;
using ShopApi.Models.Base;

namespace ShopApi.Models.Orders;

public class OrderItem : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string ProductName { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
    public decimal UnitPrice { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; }

    public override string ToString()
    {
        return $"ItemId:{Id}, OrderId:{OrderId}, Product:{ProductName}, " +
               $"Quantity:{Quantity}, Price:{UnitPrice}";
    }
}