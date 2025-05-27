using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ArtworkStoreApi.DTOs;
using ArtworkStoreApi.Extensions;
using ArtworkStoreApi.Models;
using ArtworkStoreApi.Repositories;
using ArtworkStoreApi.Services;
using ArtworkStoreApi.Utils;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtworkStoreApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IGenericService<Order, OrderDto, OrderCreateDto> _orderService;
        private readonly IGenericRepository<Order> _orderRepository;
        private readonly IGenericRepository<Artwork> _artworkRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;
        private readonly IAppLogger _logger;

        public OrdersController(
            IGenericService<Order, OrderDto, OrderCreateDto> orderService,
            IGenericRepository<Order> orderRepository,
            IGenericRepository<Artwork> artworkRepository,
            IGenericRepository<User> userRepository,
            IEmailSender emailSender,
            IMapper mapper,
            IAppLogger logger)
        {
            _orderService = orderService;
            _orderRepository = orderRepository;
            _artworkRepository = artworkRepository;
            _userRepository = userRepository;
            _emailSender = emailSender;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Získá objednávky aktuálního uživatele
        /// </summary>
        [HttpGet("my-orders")]
        public async Task<ActionResult<ResultDto<IEnumerable<OrderDto>>>> GetMyOrders()
        {
            try
            {
                var userId = GetCurrentUserId();
                var orders = await _orderRepository.GetOrdersByUserAsync(userId);
                var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);
                var result = ResultDto<IEnumerable<OrderDto>>.Success(orderDtos, "Orders retrieved successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting user orders", ex);
                var errorResult = ResultDto<IEnumerable<OrderDto>>.Failure("Failed to retrieve orders", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Získá všechny objednávky (pouze Admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResultDto<IEnumerable<OrderDto>>>> GetAllOrders()
        {
            try
            {
                var result = await _orderService.GetAllAsync();
                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting all orders", ex);
                var errorResult = ResultDto<IEnumerable<OrderDto>>.Failure("Failed to retrieve orders", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Získá objednávky podle statusu (pouze Admin)
        /// </summary>
        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResultDto<IEnumerable<OrderDto>>>> GetOrdersByStatus(string status)
        {
            try
            {
                var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
                if (!validStatuses.Contains(status))
                {
                    var validationResult = ResultDto<IEnumerable<OrderDto>>.Failure("Invalid status. Valid statuses: " + string.Join(", ", validStatuses));
                    return BadRequest(validationResult);
                }

                var orders = await _orderRepository.GetOrdersByStatusAsync(status);
                var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);
                var result = ResultDto<IEnumerable<OrderDto>>.Success(orderDtos, $"Orders with status '{status}' retrieved successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting orders by status {status}", ex);
                var errorResult = ResultDto<IEnumerable<OrderDto>>.Failure("Failed to retrieve orders", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Získá konkrétní objednávku podle ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ResultDto<OrderDto>>> GetById(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = User.IsInRole("Admin");
                
                var order = await _orderRepository.GetByIdAsync(id);
                
                if (order == null)
                {
                    var notFoundResult = ResultDto<OrderDto>.Failure("Order not found");
                    return NotFound(notFoundResult);
                }

                // Uživatel může vidět pouze své objednávky, admin může vidět všechny
                if (!isAdmin && order.UserId != userId)
                {
                    var forbidResult = ResultDto<OrderDto>.Failure("You can only view your own orders");
                    return Forbid();
                }

                var orderDto = _mapper.Map<OrderDto>(order);
                var result = ResultDto<OrderDto>.Success(orderDto, "Order retrieved successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting order {id}", ex);
                var errorResult = ResultDto<OrderDto>.Failure("Failed to retrieve order", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Vytvoří novou objednávku
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ResultDto<OrderDto>>> CreateOrder([FromBody] OrderCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    var validationResult = ResultDto<OrderDto>.Failure("Validation failed", errors);
                    return BadRequest(validationResult);
                }

                var userId = GetCurrentUserId();
                var user = await _userRepository.GetByIdAsync(userId);
                
                if (user == null || !user.IsActive)
                {
                    var userResult = ResultDto<OrderDto>.Failure("User account is not active");
                    return BadRequest(userResult);
                }

                // Vytvoření objednávky
                var order = new Order
                {
                    UserId = userId,
                    ShippingAddress = createDto.ShippingAddress,
                    Status = "Pending",
                    OrderDate = DateTime.UtcNow
                };

                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                // Validace a vytvoření order items
                foreach (var itemDto in createDto.OrderItems)
                {
                    var artwork = await _artworkRepository.GetByIdAsync(itemDto.ArtworkId);
                    if (artwork == null)
                    {
                        var artworkResult = ResultDto<OrderDto>.Failure($"Artwork with ID {itemDto.ArtworkId} not found");
                        return BadRequest(artworkResult);
                    }

                    if (!artwork.IsAvailable)
                    {
                        var availabilityResult = ResultDto<OrderDto>.Failure($"Artwork '{artwork.Title}' is not available");
                        return BadRequest(availabilityResult);
                    }

                    var orderItem = new OrderItem
                    {
                        ArtworkId = itemDto.ArtworkId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = artwork.Price
                    };

                    orderItems.Add(orderItem);
                    totalAmount += orderItem.TotalPrice;
                }

                order.TotalAmount = totalAmount;
                order.OrderItems = orderItems;

                var createdOrder = await _orderRepository.AddAsync(order);
                var orderDto = _mapper.Map<OrderDto>(createdOrder);

                // Odeslání potvrzovacího emailu
                try
                {
                    await _emailSender.SendOrderConfirmationAsync(user.Email, orderDto);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning($"Failed to send order confirmation email to {user.Email}", emailEx);
                }

                _logger.LogInfo($"Order {createdOrder.Id} created for user {userId}");
                var result = ResultDto<OrderDto>.Success(orderDto, "Order created successfully");
                return CreatedAtAction(nameof(GetById), new { id = orderDto.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating order", ex);
                var errorResult = ResultDto<OrderDto>.Failure("Failed to create order", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Aktualizuje status objednávky (pouze Admin)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResultDto<object>>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto statusDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    var validationResult = ResultDto<object>.Failure("Validation failed", errors);
                    return BadRequest(validationResult);
                }

                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    var notFoundResult = ResultDto<object>.Failure("Order not found");
                    return NotFound(notFoundResult);
                }

                var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
                if (!validStatuses.Contains(statusDto.Status))
                {
                    var statusResult = ResultDto<object>.Failure("Invalid status. Valid statuses: " + string.Join(", ", validStatuses));
                    return BadRequest(statusResult);
                }

                var oldStatus = order.Status;
                order.Status = statusDto.Status;
                await _orderRepository.UpdateAsync(order);

                _logger.LogInfo($"Order {id} status updated from {oldStatus} to {statusDto.Status}");
                
                var result = ResultDto<object>.Success(
                    new { OrderId = id, NewStatus = statusDto.Status, PreviousStatus = oldStatus }, 
                    "Order status updated successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating order {id} status", ex);
                var errorResult = ResultDto<object>.Failure("Failed to update order status", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Zruší objednávku (pouze pokud je ve stavu Pending)
        /// </summary>
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<ResultDto<object>>> CancelOrder(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = User.IsInRole("Admin");
                
                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    var notFoundResult = ResultDto<object>.Failure("Order not found");
                    return NotFound(notFoundResult);
                }

                // Uživatel může zrušit pouze své objednávky, admin může zrušit jakékoliv
                if (!isAdmin && order.UserId != userId)
                {
                    return Forbid();
                }

                if (order.Status != "Pending")
                {
                    var statusResult = ResultDto<object>.Failure($"Cannot cancel order with status '{order.Status}'. Only pending orders can be cancelled.");
                    return BadRequest(statusResult);
                }

                order.Status = "Cancelled";
                await _orderRepository.UpdateAsync(order);

                _logger.LogInfo($"Order {id} cancelled by user {userId}");
                
                var result = ResultDto<object>.Success(
                    new { OrderId = id, Status = "Cancelled" }, 
                    "Order cancelled successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error cancelling order {id}", ex);
                var errorResult = ResultDto<object>.Failure("Failed to cancel order", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Získá statistiky objednávek (pouze Admin)
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResultDto<object>>> GetOrderStatistics()
        {
            try
            {
                var allOrders = await _orderRepository.GetAllAsync();
                
                var statistics = new
                {
                    TotalOrders = allOrders.Count(),
                    PendingOrders = allOrders.Count(o => o.Status == "Pending"),
                    ProcessingOrders = allOrders.Count(o => o.Status == "Processing"),
                    ShippedOrders = allOrders.Count(o => o.Status == "Shipped"),
                    DeliveredOrders = allOrders.Count(o => o.Status == "Delivered"),
                    CancelledOrders = allOrders.Count(o => o.Status == "Cancelled"),
                    TotalRevenue = allOrders.Where(o => o.Status == "Delivered").Sum(o => o.TotalAmount),
                    AverageOrderValue = allOrders.Any() ? allOrders.Average(o => o.TotalAmount) : 0,
                    OrdersThisMonth = allOrders.Count(o => o.OrderDate >= DateTime.UtcNow.AddDays(-30))
                };

                var result = ResultDto<object>.Success(statistics, "Order statistics retrieved successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting order statistics", ex);
                var errorResult = ResultDto<object>.Failure("Failed to retrieve order statistics", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private string GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        }
    }

    // DTO pro update status
    public class UpdateOrderStatusDto
    {
        [Required]
        public string Status { get; set; }
    }
}
