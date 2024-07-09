using ShopApi.PublicModels.Orders;

namespace ShopApi.Services.Interfaces;

public interface IMessageQueueService
{
    void PublishPaymentMessage(PaymentInfoDto paymentInfo);
}