using WijkMeld.API.Entities;
using WijkMeld.API.Entities.Enums;
using WijkMeld.API.Repositories.Incidents; 
using WijkMeld.API.Repositories.StatusUpdates; 
using WijkMeld.API.Repositories.Users;

namespace WijkMeld.API.Services
{
    public class IncidentService
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly IStatusUpdateRepository _statusUpdateRepository;
        private readonly IUserRepository _userRepository;


        public IncidentService(
            IIncidentRepository incidentRepository,
            IStatusUpdateRepository statusUpdateRepository, 
            IUserRepository userRepository )
        {
            _incidentRepository = incidentRepository;
            _statusUpdateRepository = statusUpdateRepository;
            _userRepository = userRepository;
        }

        public async Task<bool> UpdateIncidentStatusAsync(
            Guid incidentId,
            Guid ChangedByUserId,
            Status? newStatus = null,
            string? note = null,
            Priority? newPriority = null)
        {
            var incident = await _incidentRepository.GetByIdAsync(incidentId);
            if (incident == null)
            {
                throw new ArgumentException("Incident not found");
            }


            var changedByUser = await _userRepository.GetByIdAsync(ChangedByUserId);
            if (changedByUser == null)
            {
                throw new ArgumentException("The user that is changing the incident is not found");
            }

            var statusUpdate = new StatusUpdate
            {
                Id = Guid.NewGuid(), // Genereer een nieuwe ID voor de StatusUpdate
                IncidentId = incidentId,
                //NewStatus = newStatus,
                ChangedBy = changedByUser, // Wijs de User entity toe
                Date = DateTime.UtcNow, // Gebruik UTC voor consistentie
                Note = note,
                NewPrio = newPriority
            };

            await _statusUpdateRepository.AddAsync(statusUpdate);

            if (newStatus.HasValue)
            {
                incident.Status = newStatus.Value;
            }
            if (newPriority.HasValue)
            {
                incident.Priority = newPriority.Value;
            }
            await _incidentRepository.UpdateAsync(incident);

            return true;


        }
            



    }
}
