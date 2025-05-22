using Microsoft.EntityFrameworkCore;
using WijkMeld.API.Data;
using WijkMeld.API.Entities;
using WijkMeld.API.Entities.Enums;

namespace WijkMeld.API.Repositories.Incidents
{
    public class IncidentRepository : IIncidentRepository
    {


        private readonly WijkMeldContext _context;

        public IncidentRepository(WijkMeldContext context)
        {
            _context = context;
        }   

        public async Task AddAsync(Incident incident)
        {
            incident.Id = Guid.NewGuid();
            
            _context.Incidents.Add(incident);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var incident = await GetByIdAsync(id);
            if (incident != null)
            {
                _context.Incidents.Remove(incident);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Incident>> GetAllAsync()
        {
            return await _context.Incidents
                 .Include(i => i.User)
                 .Include(i => i.Location)
                 .Include(i=> i.StatusUpdates)
                 .ToListAsync();
        }
            

        public async Task<Incident?> GetByIdAsync(Guid id)
           {
            return await _context.Incidents
                     .Include(i => i.User)
                     .Include(i => i.Location)
                     .Include(i => i.StatusUpdates)
                     .FirstOrDefaultAsync(i => i.Id == id);
            }

        public async Task UpdatePrioAsync(Guid incidentId, Priority newPriority)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateStatusAsync(Guid incidentId, Status newStatus)
        {
            
        }

        public async Task UpdateAsync(Incident incident)
        {
            _context.Incidents.Update(incident);
            await _context.SaveChangesAsync();
        }
    }
}
