using Microsoft.EntityFrameworkCore;
using WijkMeld.API.Data;
using WijkMeld.API.Entities;

namespace WijkMeld.API.Repositories.StatusUpdates
{
    public class StatusUpdateRepository : IStatusUpdateRepository
    {
        private readonly WijkMeldContext _context;

        public StatusUpdateRepository(WijkMeldContext context)
        {
            _context = context;
        }

        public async Task AddAsync(StatusUpdate statusUpdate)
        {
            if (statusUpdate.Id == Guid.Empty)
            {
                statusUpdate.Id = Guid.NewGuid();
            }
            _context.StatusUpdates.Add(statusUpdate);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var statusUpdate = await GetByIdAsync(id);
            if (statusUpdate != null)
            {
                _context.StatusUpdates.Remove(statusUpdate);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<StatusUpdate>> GetAllAsync()
        {
            return await _context.StatusUpdates
                                 .Include(su => su.Incident) 
                                 .Include(su => su.ChangedBy) 
                                 .ToListAsync();
        }

        public async Task<StatusUpdate?> GetByIdAsync(Guid id)
        {
            return await _context.StatusUpdates
                                 .Include(su => su.Incident)
                                 .Include(su => su.ChangedBy)
                                 .FirstOrDefaultAsync(su => su.Id == id);
        }

        public async Task<IEnumerable<StatusUpdate>> GetByIncidentIdAsync(Guid incidentId)
        {
            return await _context.StatusUpdates
                                 .Where(su => su.IncidentId == incidentId)
                                 .Include(su => su.Incident)
                                 .Include(su => su.ChangedBy)
                                 .OrderByDescending(su => su.Date) 
                                 .ToListAsync();
        }

        public async Task UpdateAsync(StatusUpdate statusUpdate)
        {
            _context.StatusUpdates.Update(statusUpdate);
            await _context.SaveChangesAsync();
        }
    }
}
