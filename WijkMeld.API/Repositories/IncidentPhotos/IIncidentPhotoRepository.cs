using WijkMeld.API.Entities;

namespace WijkMeld.API.Repositories.IncidentPhotos
{
    public interface IIncidentPhotoRepository
    {
        Task<IncidentPhoto?> GetByIdAsync(Guid id);
        Task<IEnumerable<IncidentPhoto>> GetAllAsync();
        Task<IEnumerable<IncidentPhoto>> GetByIncidentIdAsync(Guid incidentId);
        Task DeleteAsync(Guid id);

        Task AddAsync(IncidentPhoto photo);
    }
}


