using System.Text;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ShopApi.Configurations;
using ShopApi.Models;
using ShopApi.Models.Enums;
using ShopApi.Models.Orders;
using ShopApi.PublicModels.Orders;

namespace ShopApi.Services;

public class PaymentProcessingService : BackgroundService
{
    private IModel _channel;
    private IConnection _connection;

    private readonly RabbitMqConfiguration _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentProcessingService> _logger;

    public PaymentProcessingService(
        RabbitMqConfiguration config,
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentProcessingService> logger)
    {
        _config = config;
        _logger = logger;

        _scopeFactory = scopeFactory;

        InitializeRabbitMQ();
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        EventingBasicConsumer consumer = new(_channel);

        consumer.Received += ProccessMessageAsync;

        _channel.BasicConsume(
            queue: _config.QueueName,
            autoAck: true,
            consumer: consumer);

        return Task.CompletedTask;
    }

    private void InitializeRabbitMQ()
    {
        ConnectionFactory factory = new()
        {
            HostName = _config.HostName,
            UserName = _config.UserName,
            Password = _config.Password,
        };

        _connection = factory.CreateConnection();

        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: _config.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    private async void ProccessMessageAsync(object? sender, BasicDeliverEventArgs e)
    {
        try
        {
            byte[] body = e.Body.ToArray();

            using IServiceScope scope = _scopeFactory.CreateScope();

            PaymentInfoDto? message = JsonConvert.DeserializeObject<PaymentInfoDto>(Encoding.UTF8.GetString(body));

            ShopContext context = scope.ServiceProvider.GetRequiredService<ShopContext>();

            Order? order = await context.Orders.FirstOrDefaultAsync(x => x.OrderNumber == message.OrderNumber);

            if (order != null)
            {
                order.Status = message.IsPaid ? OrderStatus.Paid : OrderStatus.Cancelled;

                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in the payment processing: {ex.Message}");
        }
    }
}