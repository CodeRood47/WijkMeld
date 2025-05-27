using Microsoft.EntityFrameworkCore;
using WijkMeld.API.Data;
using WijkMeld.API.Entities;

namespace WijkMeld.API.Repositories.IncidentPhotos
{
    public class IncidentPhotoRepository : IIncidentPhotoRepository
    {
        private readonly WijkMeldContext _context;

        public  IncidentPhotoRepository(WijkMeldContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<IncidentPhoto>> GetAllAsync()
        {
            return await _context.IncidentPhotos.ToListAsync();
        }

        public async Task<IEnumerable<IncidentPhoto>> GetByIncidentIdAsync(Guid incidentId)
        {
            return await _context.IncidentPhotos
                .Where(p => p.IncidentId == incidentId)
                .ToListAsync();
        }

        public async Task<IncidentPhoto?> GetByIdAsync(Guid id)
        {
            return await _context.IncidentPhotos
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task DeleteAsync(Guid id)
        {
            var photo = await GetByIdAsync(id);
            if (photo != null)
            {
                _context.IncidentPhotos.Remove(photo);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddAsync(IncidentPhoto photo)
        {
            await _context.IncidentPhotos.AddAsync(photo);
            await _context.SaveChangesAsync();
        }

    
    }
}
