using ArtworkStoreApi.Models;

namespace ArtworkStoreApi.Repositories
{
    public interface IArtworkRepository
    {
        IEnumerable<Artwork> GetAll();
        Artwork GetById(int id);
        void Add(Artwork artwork);
        void Update(Artwork artwork);
        void Delete(int id);
    }
}
