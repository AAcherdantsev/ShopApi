using System.Text;
using System.Threading.Channels;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using ShopApi.Configurations;
using ShopApi.PublicModels.Orders;
using ShopApi.Services;

namespace ShopApi.Tests;


public class MessageQueueServiceTests
{
    private readonly Mock<IConnectionFactory> _connectionFactoryMock;
    private readonly Mock<IConnection> _connectionMock;
    private readonly Mock<IModel> _channelMock;
    private readonly MessageQueueService _service;

    private readonly RabbitMqConfiguration _config;

    public MessageQueueServiceTests()
    {
        _connectionFactoryMock = new Mock<IConnectionFactory>();
        _connectionMock = new Mock<IConnection>();
        _channelMock = new Mock<IModel>();

        _connectionFactoryMock.Setup(cf => cf.CreateConnection())
                              .Returns(_connectionMock.Object);

        _connectionMock.Setup(c => c.CreateModel())
                       .Returns(_channelMock.Object);

        _config = new()
        {
            HostName = "localhost",
            QueueName = "payments",
            UserName = "guest",
            Password = "guest"
        };

        _service = new MessageQueueService(_config);
    }

    [Fact]
    public void PublishPaymentMessage_ShouldSendPaymentMessageToQueue()
    {

        _channelMock.Setup(x => x.BasicPublish("", "payments", null, It.IsAny<byte[]>()));


        var message = new PaymentInfoDto { OrderNumber = "1", IsPaid = true };

        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        _service.PublishPaymentMessage(message);

        _channelMock.Verify(c => c.BasicPublish("", "payments", null, body), Times.Once);

    }


    [Fact]
    public void PublishPaymentMessage_ShouldHandleNullMessage()
    {
        PaymentInfoDto message = null;

        Assert.Throws<ArgumentNullException>(() => _service.PublishPaymentMessage(message));
    }
}