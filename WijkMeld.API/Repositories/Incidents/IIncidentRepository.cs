using WijkMeld.API.Entities;
using WijkMeld.API.Entities.Enums;

namespace WijkMeld.API.Repositories.Incidents
{
    public interface IIncidentRepository
    {
        Task<IEnumerable<Incident>> GetAllAsync();
        Task<Incident?> GetByIdAsync(Guid id);
        Task AddAsync(Incident incident);

        Task UpdateAsync(Incident incident);

        Task UpdatePrioAsync(Guid incidentId, Priority newPriority); 

        Task UpdateStatusAsync(Guid incidentId, Status newStatus);

        Task DeleteAsync(Guid id);
        
    }
}
