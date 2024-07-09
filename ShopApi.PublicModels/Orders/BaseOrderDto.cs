namespace ShopApi.PublicModels.Orders;

public class BaseOrderDto
{
    public required string OrderNumber { get; set; }

    public required string CustomerName { get; set; }

    public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
}