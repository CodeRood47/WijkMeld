using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WijkMeld.API.Entities.Enums;

namespace WijkMeld.API.Entities
{
    public class StatusUpdate
    {
        [Key]
        public Guid Id { get; set; }

        public Guid IncidentId { get; set; }

        public Incident Incident { get; set; }

        public Status? NewStatus { get; set; }

        public User ChangedBy { get; set; }

        public DateTime Date { get; set; }

        public string? Note { get; set; }

        public Priority? NewPrio { get; set; }
    }
}
