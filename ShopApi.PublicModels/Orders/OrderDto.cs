using ShopApi.Models.Enums;

namespace ShopApi.PublicModels.Orders;

public class OrderDto : BaseOrderDto
{
    public OrderStatus Status { get; set; }

    public DateTime Created { get; set; }
}