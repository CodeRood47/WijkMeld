using WijkMeld.API.Entities;

namespace WijkMeld.API.Repositories.StatusUpdates

{
    public interface IStatusUpdateRepository
    {
        Task<IEnumerable<StatusUpdate>> GetAllAsync();
        Task<StatusUpdate?> GetByIdAsync(Guid id);
        Task AddAsync(StatusUpdate statusUpdate);
        Task UpdateAsync(StatusUpdate statusUpdate);
        Task DeleteAsync(Guid id);
    }
}
