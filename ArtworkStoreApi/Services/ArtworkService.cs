using ArtworkStoreApi.DTOs;
using ArtworkStoreApi.Models;
using ArtworkStoreApi.Repositories;

namespace ArtworkStoreApi.Services
{
    public class ArtworkService : IArtworkService
    {
        private readonly IArtworkRepository _artworkRepository;
        public ArtworkService(IArtworkRepository artworkRepository)
        {
            _artworkRepository = artworkRepository;
        }
        public IEnumerable<ArtworkDto> GetAll()
        {
            return _artworkRepository.GetAll()
                .Select(a => new ArtworkDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    GenreId = a.GenreId,
                    Price = a.Price,
                    Description = a.Description,
                    Quantity = a.Quantity,
                    AverageRating = a.AverageRating
                });
        }
        public ArtworkDto GetById(int id)
        {
            var a = _artworkRepository.GetById(id);
            if (a == null)
                return null;
            return new ArtworkDto
            {
                Id = a.Id,
                Title = a.Title,
                GenreId = a.GenreId,
                Price = a.Price,
                Description = a.Description,
                Quantity = a.Quantity,
                AverageRating = a.AverageRating
            };
        }
        public void AddArtwork(ArtworkDto artwork)
        {
            var entity = new Artwork
            {
                Title = artwork.Title,
                GenreId = artwork.GenreId,
                Price = artwork.Price,
                Description = artwork.Description,
                Quantity = artwork.Quantity,
                AverageRating = artwork.AverageRating
            };
            _artworkRepository.Add(entity);
        }
        public void UpdateArtwork(int id, ArtworkDto artwork)
        {
            var entity = _artworkRepository.GetById(id);
            if (entity != null)
            {
                entity.Title = artwork.Title;
                entity.GenreId = artwork.GenreId;
                entity.Price = artwork.Price;
                entity.Description = artwork.Description;
                entity.Quantity = artwork.Quantity;
                entity.AverageRating = artwork.AverageRating;
            }
        }
        public void DeleteArtwork(int id)
        {
            _artworkRepository.Delete(id);
        }
    }
}
