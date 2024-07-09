using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApi.Models;
using ShopApi.Models.Orders;
using ShopApi.PublicModels.Orders;
using ShopApi.Services.Interfaces;

namespace ShopApi.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ShopContext _context;
    private readonly ILogger<OrdersController> _logger;
    private readonly IMessageQueueService _messageQueueService;

    public OrdersController(
        IMapper mapper,
        ShopContext context,
        IMessageQueueService messageQueueService,
        ILogger<OrdersController> logger)
    {
        _mapper = mapper;
        _logger = logger;
        _context = context;
        _messageQueueService = messageQueueService;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> GetOrdersAsync()
    {
        _logger.LogInformation("Retrieving all orders...");

        List<Order> orders = await _context.Orders.Include(x => x.Items).ToListAsync();

        List<OrderDto> orderDtos = _mapper.Map<List<OrderDto>>(orders);

        if (orderDtos.Count == 0)
        {
            _logger.LogWarning($"No orders found.");
            return NotFound();
        }

        return Ok(orderDtos);
    }

    [HttpGet("{orderNumber}")]
    public async Task<ActionResult<OrderDto>> GetOrderAsync(string orderNumber)
    {
        _logger.LogInformation($"Retrieving order with number {orderNumber}...");

        Order? order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(x => x.OrderNumber == orderNumber);

        if (order == null)
        {
            _logger.LogWarning($"Order with order number {orderNumber} not found.");

            return NotFound();
        }

        OrderDto orderDto = _mapper.Map<OrderDto>(order);

        return Ok(orderDto);
    }

    [HttpPost("payment")]
    public IActionResult ProcessPayment([FromBody] PaymentInfoDto paymentInfo)
    {
        _logger.LogInformation($"Processing payment for order number: {paymentInfo.OrderNumber}...");

        _messageQueueService.PublishPaymentMessage(paymentInfo);

        return Accepted();
    }

    [HttpPost]
    public async Task<ActionResult> CreateOrderAsync(BaseOrderDto orderDto)
    {
        if (orderDto.Items == null || orderDto.Items.Count == 0)
        {
            _logger.LogWarning("Attempt to create order with empty items list.");
            return BadRequest("Order must have at least one item.");
        }

        if (orderDto.Items.Any(x => x.Quantity <= 0))
        {
            _logger.LogWarning("Attempt to create order with item having non-positive quantity.");
            return BadRequest("Each item must have a positive quantity.");
        }

        Order? existingOrder = await _context.Orders
            .FirstOrDefaultAsync(x => x.OrderNumber == orderDto.OrderNumber);

        if (existingOrder != null)
        {
            _logger.LogWarning("Attempt to create an order with an existing number.");
            return BadRequest("An order with this number already exists.");
        }

        Order order = _mapper.Map<Order>(orderDto);

        _context.Orders.Add(order);

        await _context.SaveChangesAsync();

        return Ok();
    }
}