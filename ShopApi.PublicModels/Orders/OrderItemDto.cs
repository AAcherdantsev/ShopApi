namespace ShopApi.PublicModels.Orders;

public class OrderItemDto
{
    public required string ProductName { get; set; }

    public required decimal UnitPrice { get; set; }

    public required int Quantity { get; set; }
}