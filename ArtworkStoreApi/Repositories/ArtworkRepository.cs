using ArtworkStoreApi.Data;
using ArtworkStoreApi.Models;

namespace ArtworkStoreApi.Repositories
{
    public class ArtworkRepository : IArtworkRepository
    {
        private readonly DatabaseContext _context;
        public ArtworkRepository(DatabaseContext context)
        {
            _context = context;
        }
        public IEnumerable<Artwork> GetAll() => _context.Artworks.ToList();
        public Artwork GetById(int id) => _context.Artworks.Find(id);
        public void Add(Artwork artwork)
        {
            _context.Artworks.Add(artwork);
            _context.SaveChanges();
        }
        public void Update(Artwork artwork)
        {
            _context.Artworks.Update(artwork);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var artwork = _context.Artworks.Find(id);
            if (artwork != null)
            {
                _context.Artworks.Remove(artwork);
                _context.SaveChanges();
            }
        }

    }
}
