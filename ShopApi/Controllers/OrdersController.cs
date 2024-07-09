﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApi.Models;
using ShopApi.Models.Enums;
using ShopApi.Models.Orders;
using ShopApi.PublicModels.Orders;
using ShopApi.Services.Interfaces;

namespace ShopApi.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly OrderContext _context;
    private readonly ILogger<OrdersController> _logger;
    private readonly IMessageQueueService _messageQueueService;

    public OrdersController(
        IMapper mapper,
        OrderContext context,
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

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrderAsync(int id)
    {
        _logger.LogInformation($"Retrieving order with ID {id}...");

        Order? order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null)
        {
            _logger.LogWarning($"Order with ID {id} not found.");

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
    public async Task<ActionResult> CreateOrderAsync(OrderDto orderDto)
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

        orderDto.Status = OrderStatus.New;

        Order order = _mapper.Map<Order>(orderDto);

        _context.Orders.Add(order);

        await _context.SaveChangesAsync();

        return Ok();
    }
}