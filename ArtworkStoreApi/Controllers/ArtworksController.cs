using ArtworkStoreApi.DTOs;
using ArtworkStoreApi.Extensions;
using ArtworkStoreApi.Models;
using ArtworkStoreApi.Repositories;
using ArtworkStoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtworkStoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArtworksController : ControllerBase
    {
        private readonly IGenericService<Artwork, ArtworkDto, ArtworkCreateDto> _artworkService;
        private readonly IGenericRepository<Artwork> _artworkRepository;

        public ArtworksController(
            IGenericService<Artwork, ArtworkDto, ArtworkCreateDto> artworkService,
            IGenericRepository<Artwork> artworkRepository)
        {
            _artworkService = artworkService;
            _artworkRepository = artworkRepository;
        }

        [HttpGet]
        public async Task<ActionResult<ResultDto<IEnumerable<ArtworkDto>>>> GetAll()
        {
            var result = await _artworkService.GetAllAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResultDto<ArtworkDto>>> GetById(int id)
        {
            var result = await _artworkService.GetByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("genre/{genreId}")]
        public async Task<ActionResult<ResultDto<IEnumerable<ArtworkDto>>>> GetByGenre(int genreId)
        {
            try
            {
                var artworks = await _artworkRepository.GetArtworksByGenreAsync(genreId);
                var result = ResultDto<IEnumerable<ArtworkDto>>.Success(artworks, "Artworks retrieved successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                var errorResult = ResultDto<IEnumerable<ArtworkDto>>.Failure("Failed to retrieve artworks", ex.Message);
                return BadRequest(errorResult);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ResultDto<ArtworkDto>>> Create([FromBody] ArtworkCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                var validationResult = ResultDto<ArtworkDto>.Failure("Validation failed", errors);
                return BadRequest(validationResult);
            }

            var result = await _artworkService.CreateAsync(createDto);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result)
                : BadRequest(result);
        }
    }
    // existuje atribut auth... = určití Useři můžou používat tyto metody
    // Ke každé service děláš DTO zvlášť = AutoMapper

}
