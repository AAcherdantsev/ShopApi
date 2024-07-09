using Microsoft.EntityFrameworkCore;
using ShopApi.Configurations;
using ShopApi.Mapping;
using ShopApi.Models;
using ShopApi.Services;
using ShopApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddSingleton(builder.Configuration.GetSection("RabbitMQConfig").Get<RabbitMqConfiguration>());

builder.Services.AddDbContext<OrderContext>(opt => opt.UseInMemoryDatabase("OrdersDb"));

builder.Services.AddSingleton<IMessageQueueService, MessageQueueService>();
builder.Services.AddHostedService<PaymentProcessingService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();