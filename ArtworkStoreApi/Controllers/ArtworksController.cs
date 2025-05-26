using ArtworkStoreApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArtworkStoreApi.Controllers
{
    [ApiController]
    [Route("Store/[controller]")]
    public class ArtworksController : ControllerBase
    {   
        private readonly IArtworkService _artworkService;
        public ArtworksController(IArtworkService artworkService)
        {
            _artworkService = artworkService;
        }
        [HttpGet]
        public ActionResult<IEnumerable<DTOs.ArtworkDto>> GetAll()
        {
            return Ok(_artworkService.GetAll());
        }
        [HttpGet("{id}")]
        public ActionResult<DTOs.ArtworkDto> GetById(int id)
        {
            var artwork = _artworkService.GetById(id);
                if (artwork == null)
            return NotFound();
            return Ok(artwork);
        }

        // existuje atribut auth... = určití Useři můžou používat tyto metody
        // Ke každé service děláš DTO zvlášť = AutoMapper
    }
}
