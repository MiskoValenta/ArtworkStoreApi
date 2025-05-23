using ArtworkStoreApi.DTOs;

namespace ArtworkStoreApi.Services
{
    public interface IArtworkService
    {
        IEnumerable<ArtworkDto> GetAll();
        ArtworkDto GetById(int id);
        void AddArtwork(ArtworkDto artwork);
        void UpdateArtwork(int id, ArtworkDto artwork);
        void DeleteArtwork(int id);
    }
}
