using ArtworkStoreApi.DTOs;
using ArtworkStoreApi.Models;
using ArtworkStoreApi.Repositories;
using ArtworkStoreApi.Services;
using ArtworkStoreApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtworkStoreApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IGenericService<UserDto, UserDto, RegisterRequestDto> _userService;
        private readonly IGenericRepository<UserDto> _userRepository;
        private readonly IGenericRepository<Order> _orderRepository;
        private readonly IGenericRepository<Review> _reviewRepository;
        private readonly IAppLogger _logger;

        public AdminController(
            IGenericService<UserDto, UserDto, RegisterRequestDto> userService,
            IGenericRepository<UserDto> userRepository,
            IGenericRepository<Order> orderRepository,
            IGenericRepository<Review> reviewRepository,
            IAppLogger logger)
        {
            _userService = userService;
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _reviewRepository = reviewRepository;
            _logger = logger;
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var result = await _userService.GetAllAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Message);
        }

        [HttpPut("users/{id}/toggle-status")]
        public async Task<ActionResult> ToggleUserStatus(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound("User not found");

            user.IsActive = !user.IsActive;
            await _userRepository.UpdateAsync(user);
            
            _logger.LogInfo($"User {id} status toggled to {user.IsActive}");
            return Ok(new { message = "User status updated", isActive = user.IsActive });
        }

        [HttpGet("statistics")]
        public async Task<ActionResult> GetStatistics()
        {
            var totalUsers = (await _userRepository.GetAllAsync()).Count();
            var totalOrders = (await _orderRepository.GetAllAsync()).Count();
            var pendingReviews = (await _reviewRepository.FindAsync(r => !r.IsApproved)).Count();

            return Ok(new
            {
                TotalUsers = totalUsers,
                TotalOrders = totalOrders,
                PendingReviews = pendingReviews
            });
        }

        [HttpPut("reviews/{id}/approve")]
        public async Task<ActionResult> ApproveReview(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
                return NotFound("Review not found");

            review.IsApproved = true;
            await _reviewRepository.UpdateAsync(review);
            
            return Ok(new { message = "Review approved" });
        }
    }
}
