using ShopApi.Models.Enums;

namespace ShopApi.PublicModels.Orders;

public class OrderDto
{
    public required string OrderNumber { get; set; }

    public required string CustomerName { get; set; }

    public OrderStatus Status { get; set; }

    public DateTime Created { get; set; }

    public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
}