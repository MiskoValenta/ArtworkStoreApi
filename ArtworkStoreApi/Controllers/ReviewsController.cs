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
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IGenericService<Review, ReviewDto, ReviewCreateDto> _reviewService;
        private readonly IGenericRepository<Review> _reviewRepository;
        private readonly IGenericRepository<Artwork> _artworkRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IMapper _mapper;
        private readonly IAppLogger _logger;

        public ReviewsController(
            IGenericService<Review, ReviewDto, ReviewCreateDto> reviewService,
            IGenericRepository<Review> reviewRepository,
            IGenericRepository<Artwork> artworkRepository,
            IGenericRepository<User> userRepository,
            IMapper mapper,
            IAppLogger logger)
        {
            _reviewService = reviewService;
            _reviewRepository = reviewRepository;
            _artworkRepository = artworkRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Získá schválené recenze pro konkrétní umělecké dílo
        /// </summary>
        [HttpGet("artwork/{artworkId}")]
        public async Task<ActionResult<ResultDto<IEnumerable<ReviewDto>>>> GetByArtwork(int artworkId)
        {
            try
            {
                if (artworkId <= 0)
                {
                    var validationResult = ResultDto<IEnumerable<ReviewDto>>.Failure("Invalid artwork ID");
                    return BadRequest(validationResult);
                }

                // Zkontroluj, že artwork existuje
                var artwork = await _artworkRepository.GetByIdAsync(artworkId);
                if (artwork == null)
                {
                    var notFoundResult = ResultDto<IEnumerable<ReviewDto>>.Failure("Artwork not found");
                    return NotFound(notFoundResult);
                }

                var reviews = await _reviewRepository.GetReviewsByArtworkAsync(artworkId);
                var approvedReviews = reviews.Where(r => r.IsApproved).OrderByDescending(r => r.CreatedAt);
                var reviewDtos = _mapper.Map<IEnumerable<ReviewDto>>(approvedReviews);
                
                var result = ResultDto<IEnumerable<ReviewDto>>.Success(reviewDtos, "Reviews retrieved successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting reviews for artwork {artworkId}", ex);
                var errorResult = ResultDto<IEnumerable<ReviewDto>>.Failure("Failed to retrieve reviews", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Získá průměrné hodnocení a počet recenzí pro umělecké dílo
        /// </summary>
        [HttpGet("artwork/{artworkId}/rating")]
        public async Task<ActionResult<ResultDto<object>>> GetAverageRating(int artworkId)
        {
            try
            {
                if (artworkId <= 0)
                {
                    var validationResult = ResultDto<object>.Failure("Invalid artwork ID");
                    return BadRequest(validationResult);
                }

                var artwork = await _artworkRepository.GetByIdAsync(artworkId);
                if (artwork == null)
                {
                    var notFoundResult = ResultDto<object>.Failure("Artwork not found");
                    return NotFound(notFoundResult);
                }

                var averageRating = await _reviewRepository.GetAverageRatingAsync(artworkId);
                var reviews = await _reviewRepository.GetReviewsByArtworkAsync(artworkId);
                var reviewCount = reviews.Count(r => r.IsApproved);

                var ratingData = new
                {
                    ArtworkId = artworkId,
                    AverageRating = Math.Round(averageRating, 2),
                    ReviewCount = reviewCount,
                    RatingDistribution = new
                    {
                        FiveStar = reviews.Count(r => r.IsApproved && r.Rating == 5),
                        FourStar = reviews.Count(r => r.IsApproved && r.Rating == 4),
                        ThreeStar = reviews.Count(r => r.IsApproved && r.Rating == 3),
                        TwoStar = reviews.Count(r => r.IsApproved && r.Rating == 2),
                        OneStar = reviews.Count(r => r.IsApproved && r.Rating == 1)
                    }
                };

                var result = ResultDto<object>.Success(ratingData, "Rating information retrieved successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting rating for artwork {artworkId}", ex);
                var errorResult = ResultDto<object>.Failure("Failed to retrieve rating", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Vytvoří novou recenzi (pouze přihlášení uživatelé)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ResultDto<ReviewDto>>> CreateReview([FromBody] ReviewCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    var validationResult = ResultDto<ReviewDto>.Failure("Validation failed", errors);
                    return BadRequest(validationResult);
                }

                var userId = GetCurrentUserId();
                var user = await _userRepository.GetByIdAsync(userId);
                
                if (user == null || !user.IsActive)
                {
                    var userResult = ResultDto<ReviewDto>.Failure("User account is not active");
                    return BadRequest(userResult);
                }

                // Zkontroluj, že artwork existuje
                var artwork = await _artworkRepository.GetByIdAsync(createDto.ArtworkId);
                if (artwork == null)
                {
                    var artworkResult = ResultDto<ReviewDto>.Failure("Artwork not found");
                    return NotFound(artworkResult);
                }

                // Zkontroluj, že uživatel ještě nerecenzoval toto dílo
                var existingReview = (await _reviewRepository.FindAsync(r => 
                    r.UserId == userId && r.ArtworkId == createDto.ArtworkId)).FirstOrDefault();
                    
                if (existingReview != null)
                {
                    var duplicateResult = ResultDto<ReviewDto>.Failure("You have already reviewed this artwork");
                    return BadRequest(duplicateResult);
                }

                var review = new Review
                {
                    UserId = userId,
                    ArtworkId = createDto.ArtworkId,
                    Rating = createDto.Rating,
                    Comment = createDto.Comment?.Trim(),
                    IsApproved = false, // Vyžaduje schválení administrátorem
                    CreatedAt = DateTime.UtcNow
                };

                var createdReview = await _reviewRepository.AddAsync(review);
                var reviewDto = _mapper.Map<ReviewDto>(createdReview);

                _logger.LogInfo($"Review created by user {userId} for artwork {createDto.ArtworkId}");
                
                var result = ResultDto<ReviewDto>.Success(reviewDto, "Review created successfully and is pending approval");
                return CreatedAtAction(nameof(GetByArtwork), 
                    new { artworkId = createDto.ArtworkId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating review", ex);
                var errorResult = ResultDto<ReviewDto>.Failure("Failed to create review", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Získá recenze čekající na schválení (pouze Admin)
        /// </summary>
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResultDto<IEnumerable<ReviewDto>>>> GetPendingReviews()
        {
            try
            {
                var pendingReviews = await _reviewRepository.FindAsync(r => !r.IsApproved);
                var reviewDtos = _mapper.Map<IEnumerable<ReviewDto>>(pendingReviews.OrderBy(r => r.CreatedAt));
                
                var result = ResultDto<IEnumerable<ReviewDto>>.Success(reviewDtos, "Pending reviews retrieved successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting pending reviews", ex);
                var errorResult = ResultDto<IEnumerable<ReviewDto>>.Failure("Failed to retrieve pending reviews", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Schválí recenzi (pouze Admin)
        /// </summary>
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResultDto<object>>> ApproveReview(int id)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(id);
                if (review == null)
                {
                    var notFoundResult = ResultDto<object>.Failure("Review not found");
                    return NotFound(notFoundResult);
                }

                if (review.IsApproved)
                {
                    var alreadyApprovedResult = ResultDto<object>.Failure("Review is already approved");
                    return BadRequest(alreadyApprovedResult);
                }

                review.IsApproved = true;
                await _reviewRepository.UpdateAsync(review);

                _logger.LogInfo($"Review {id} approved by admin");
                
                var result = ResultDto<object>.Success(
                    new { ReviewId = id, IsApproved = true }, 
                    "Review approved successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error approving review {id}", ex);
                var errorResult = ResultDto<object>.Failure("Failed to approve review", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Zamítne recenzi (pouze Admin)
        /// </summary>
        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResultDto<object>>> RejectReview(int id)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(id);
                if (review == null)
                {
                    var notFoundResult = ResultDto<object>.Failure("Review not found");
                    return NotFound(notFoundResult);
                }

                await _reviewRepository.DeleteAsync(id);

                _logger.LogInfo($"Review {id} rejected and deleted by admin");
                
                var result = ResultDto<object>.Success(
                    new { ReviewId = id }, 
                    "Review rejected and deleted successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error rejecting review {id}", ex);
                var errorResult = ResultDto<object>.Failure("Failed to reject review", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Smaže recenzi (vlastník nebo Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ResultDto<object>>> DeleteReview(int id)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(id);
                if (review == null)
                {
                    var notFoundResult = ResultDto<object>.Failure("Review not found");
                    return NotFound(notFoundResult);
                }

                var userId = GetCurrentUserId();
                var isAdmin = User.IsInRole("Admin");

                if (review.UserId != userId && !isAdmin)
                {
                    return Forbid();
                }

                await _reviewRepository.DeleteAsync(id);

                _logger.LogInfo($"Review {id} deleted by user {userId}");
                
                var result = ResultDto<object>.Success(
                    new { ReviewId = id }, 
                    "Review deleted successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting review {id}", ex);
                var errorResult = ResultDto<object>.Failure("Failed to delete review", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Získá recenze aktuálního uživatele
        /// </summary>
        [HttpGet("my-reviews")]
        [Authorize]
        public async Task<ActionResult<ResultDto<IEnumerable<ReviewDto>>>> GetMyReviews()
        {
            try
            {
                var userId = GetCurrentUserId();
                var userReviews = await _reviewRepository.FindAsync(r => r.UserId == userId);
                var reviewDtos = _mapper.Map<IEnumerable<ReviewDto>>(userReviews.OrderByDescending(r => r.CreatedAt));
                
                var result = ResultDto<IEnumerable<ReviewDto>>.Success(reviewDtos, "Your reviews retrieved successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting user reviews", ex);
                var errorResult = ResultDto<IEnumerable<ReviewDto>>.Failure("Failed to retrieve your reviews", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
    }
}
