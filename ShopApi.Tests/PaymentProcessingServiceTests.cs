using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ShopApi.Configurations;
using ShopApi.Models;
using ShopApi.Models.Enums;
using ShopApi.Models.Orders;
using ShopApi.PublicModels.Orders;
using ShopApi.Services;

public class PaymentProcessingServiceTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IServiceProvider> _providerMock;
    private readonly Mock<OrderContext> _dbContextMock;
    private readonly Mock<IConnectionFactory> _connectionFactoryMock;
    private readonly Mock<IConnection> _connectionMock;
    private readonly Mock<IModel> _channelMock;
    private readonly PaymentProcessingService _service;
    private readonly RabbitMqConfiguration _config;


    private readonly Mock<ILogger<PaymentProcessingService>> _logger;


    public PaymentProcessingServiceTests()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _providerMock = new Mock<IServiceProvider>();
        _dbContextMock = new Mock<OrderContext>();
        _connectionFactoryMock = new Mock<IConnectionFactory>();
        _connectionMock = new Mock<IConnection>();
        _channelMock = new Mock<IModel>();
        _logger = new Mock<ILogger<PaymentProcessingService>>();

        _scopeFactoryMock.Setup(sf => sf.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.Setup(s => s.ServiceProvider).Returns(_providerMock.Object);

        _providerMock.Setup(p => p.GetService(typeof(OrderContext))).Returns(_dbContextMock.Object);

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

        _service = new PaymentProcessingService(_config, _scopeFactoryMock.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldProcessPaymentMessage()
    {
        // Arrange
        var message = new PaymentInfoDto { OrderNumber = "1", IsPaid = true };
        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
        var ea = new BasicDeliverEventArgs { Body = body };

        _channelMock.Setup(c => c.BasicConsume(
             "payments", true, It.IsAny<EventingBasicConsumer>()))

            .Callback<string, bool, IBasicConsumer>((queue, autoAck, consumer) =>
            {
                var eventingConsumer = (EventingBasicConsumer)consumer;
                eventingConsumer.HandleBasicDeliver(
                    consumerTag: "",
                    deliveryTag: 1,
                    redelivered: false,
                    exchange: "",
                    routingKey: "",
                    properties: null,
                    body: body);
            });

        var order = new Order { OrderNumber = "1", Status = OrderStatus.New };

        _dbContextMock.Setup(db => db.Orders.FindAsync(1)).ReturnsAsync(order);

        var cancellationTokenSource = new CancellationTokenSource();

        cancellationTokenSource.CancelAfter(1000);

        await _service.StartAsync(cancellationTokenSource.Token);

        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        Assert.Equal(OrderStatus.Paid, order.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotProcessIfOrderNotFound()
    {
        // Arrange
        var message = new PaymentInfoDto { OrderNumber = "1", IsPaid = true };
        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        _channelMock.Setup(c => c.BasicConsume(
            "payments", true, It.IsAny<EventingBasicConsumer>()))

            .Callback<string, bool, IBasicConsumer>((queue, autoAck, consumer) =>
            {
                var eventingConsumer = (EventingBasicConsumer)consumer;
                eventingConsumer.HandleBasicDeliver(
                    consumerTag: "",
                    deliveryTag: 1,
                    redelivered: false,
                    exchange: "",
                    routingKey: "",
                    properties: null,
                    body: body);
            });

        _dbContextMock.Setup(db => db.Orders.FindAsync(1)).ReturnsAsync((Order)null);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(1000);

        await _service.StartAsync(cancellationTokenSource.Token);

        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async void Dispose_ShouldCloseChannelAndConnection()
    {
        _service.Dispose();
        _channelMock.Verify(c => c.Close(), Times.Once);
        _connectionMock.Verify(c => c.Close(), Times.Once);
    }
}