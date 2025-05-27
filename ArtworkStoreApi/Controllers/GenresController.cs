using ArtworkStoreApi.DTOs;
using ArtworkStoreApi.Extensions;
using ArtworkStoreApi.Models;
using ArtworkStoreApi.Repositories;
using ArtworkStoreApi.Services;
using ArtworkStoreApi.Utils;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtworkStoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenresController : ControllerBase
    {
        private readonly IGenericService<Genre, GenreDto, GenreCreateDto> _genreService;
        private readonly IGenericRepository<Genre> _genreRepository;
        private readonly IMapper _mapper;
        private readonly IAppLogger _logger;

        public GenresController(
            IGenericService<Genre, GenreDto, GenreCreateDto> genreService,
            IGenericRepository<Genre> genreRepository,
            IMapper mapper,
            IAppLogger logger)
        {
            _genreService = genreService;
            _genreRepository = genreRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Získá všechny žánry
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ResultDto<IEnumerable<GenreDto>>>> GetAll()
        {
            try
            {
                var result = await _genreService.GetAllAsync();
                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting all genres", ex);
                var errorResult = ResultDto<IEnumerable<GenreDto>>.Failure("Failed to retrieve genres", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Získá pouze aktivní žánry
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<ResultDto<IEnumerable<GenreDto>>>> GetActiveGenres()
        {
            try
            {
                var activeGenres = await _genreRepository.GetActiveGenresAsync();
                var genreDtos = _mapper.Map<IEnumerable<GenreDto>>(activeGenres);
                var result = ResultDto<IEnumerable<GenreDto>>.Success(genreDtos, "Active genres retrieved successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting active genres", ex);
                var errorResult = ResultDto<IEnumerable<GenreDto>>.Failure("Failed to retrieve active genres", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Získá žánr podle ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ResultDto<GenreDto>>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    var validationResult = ResultDto<GenreDto>.Failure("Invalid genre ID");
                    return BadRequest(validationResult);
                }

                var result = await _genreService.GetByIdAsync(id);
                return result.IsSuccess ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting genre {id}", ex);
                var errorResult = ResultDto<GenreDto>.Failure("Failed to retrieve genre", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Vytvoří nový žánr (pouze Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResultDto<GenreDto>>> Create([FromBody] GenreCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    var validationResult = ResultDto<GenreDto>.Failure("Validation failed", errors);
                    return BadRequest(validationResult);
                }

                // Check if genre with same name already exists
                var existingGenres = await _genreRepository.FindAsync(g => g.Name.ToLower() == createDto.Name.ToLower());
                if (existingGenres.Any())
                {
                    var duplicateResult = ResultDto<GenreDto>.Failure("Genre with this name already exists");
                    return BadRequest(duplicateResult);
                }

                var result = await _genreService.CreateAsync(createDto);
                if (result.IsSuccess)
                {
                    _logger.LogInfo($"Genre '{createDto.Name}' created successfully");
                    return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating genre", ex);
                var errorResult = ResultDto<GenreDto>.Failure("Failed to create genre", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Aktualizuje žánr (pouze Admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResultDto<GenreDto>>> Update(int id, [FromBody] GenreCreateDto updateDto)
        {
            try
            {
                if (id <= 0)
                {
                    var validationResult = ResultDto<GenreDto>.Failure("Invalid genre ID");
                    return BadRequest(validationResult);
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    var validationResult = ResultDto<GenreDto>.Failure("Validation failed", errors);
                    return BadRequest(validationResult);
                }

                // Check if another genre with same name exists
                var existingGenres = await _genreRepository.FindAsync(g => g.Name.ToLower() == updateDto.Name.ToLower() && g.Id != id);
                if (existingGenres.Any())
                {
                    var duplicateResult = ResultDto<GenreDto>.Failure("Another genre with this name already exists");
                    return BadRequest(duplicateResult);
                }

                var result = await _genreService.UpdateAsync(id, updateDto);
                if (result.IsSuccess)
                {
                    _logger.LogInfo($"Genre {id} updated successfully");
                }
                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating genre {id}", ex);
                var errorResult = ResultDto<GenreDto>.Failure("Failed to update genre", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Smaže žánr (pouze Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResultDto<bool>>> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    var validationResult = ResultDto<bool>.Failure("Invalid genre ID");
                    return BadRequest(validationResult);
                }

                // Check if genre has associated artworks
                var genre = await _genreRepository.GetByIdAsync(id);
                if (genre == null)
                {
                    var notFoundResult = ResultDto<bool>.Failure("Genre not found");
                    return NotFound(notFoundResult);
                }

                var artworksCount = (await _genreRepository.Query()
                    .Where(g => g.Id == id)
                    .SelectMany(g => g.Artworks)
                    .ToListAsync()).Count;

                if (artworksCount > 0)
                {
                    var hasArtworksResult = ResultDto<bool>.Failure($"Cannot delete genre. It has {artworksCount} associated artworks.");
                    return BadRequest(hasArtworksResult);
                }

                var result = await _genreService.DeleteAsync(id);
                if (result.IsSuccess)
                {
                    _logger.LogInfo($"Genre {id} deleted successfully");
                }
                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting genre {id}", ex);
                var errorResult = ResultDto<bool>.Failure("Failed to delete genre", ex.Message);
                return StatusCode(500, errorResult);
            }
        }

        /// <summary>
        /// Získá statistiky žánru (pouze Admin)
        /// </summary>
        [HttpGet("{id}/statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResultDto<object>>> GetGenreStatistics(int id)
        {
            try
            {
                var genre = await _genreRepository.GetByIdAsync(id);
                if (genre == null)
                {
                    var notFoundResult = ResultDto<object>.Failure("Genre not found");
                    return NotFound(notFoundResult);
                }

                var artworks = await _genreRepository.Query()
                    .Where(g => g.Id == id)
                    .SelectMany(g => g.Artworks)
                    .ToListAsync();

                var statistics = new
                {
                    GenreId = id,
                    GenreName = genre.Name,
                    TotalArtworks = artworks.Count,
                    AvailableArtworks = artworks.Count(a => a.IsAvailable),
                    FeaturedArtworks = artworks.Count(a => a.IsFeatured),
                    AveragePrice = artworks.Any() ? artworks.Average(a => a.Price) : 0,
                    MinPrice = artworks.Any() ? artworks.Min(a => a.Price) : 0,
                    MaxPrice = artworks.Any() ? artworks.Max(a => a.Price) : 0
                };

                var result = ResultDto<object>.Success(statistics, "Genre statistics retrieved successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting statistics for genre {id}", ex);
                var errorResult = ResultDto<object>.Failure("Failed to retrieve genre statistics", ex.Message);
                return StatusCode(500, errorResult);
            }
        }
    }
}