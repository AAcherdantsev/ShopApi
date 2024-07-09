namespace ShopApi.PublicModels.Orders;

public class PaymentInfoDto
{
    public required string OrderNumber { get; set; }

    public required bool IsPaid { get; set; }
}