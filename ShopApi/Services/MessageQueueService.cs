using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using ShopApi.Configurations;
using ShopApi.PublicModels.Orders;
using ShopApi.Services.Interfaces;

namespace ShopApi.Services;

public class MessageQueueService : IMessageQueueService
{
    private ConnectionFactory _connectionFactory;

    private readonly RabbitMqConfiguration _config;

    public MessageQueueService(RabbitMqConfiguration config)
    {
        _config = config;

        _connectionFactory = new()
        {
            HostName = _config.HostName,
            UserName = _config.UserName,
            Password = _config.Password
        };
    }

    public void PublishPaymentMessage(PaymentInfoDto message)
    {
        ArgumentNullException.ThrowIfNull(message);

        using IConnection connection = _connectionFactory.CreateConnection();

        using IModel channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: _config.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        channel.BasicPublish(
            "",
            _config.QueueName,
             null,
            body);
    }
}