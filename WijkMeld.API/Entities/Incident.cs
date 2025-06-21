using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WijkMeld.API.Entities.Enums;

namespace WijkMeld.API.Entities
{
    public class Incident
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public required string Name { get; set; }
        [Required]
        public required string Description { get; set; }
        public Guid? UserId { get; set; }

        public User? User { get; set; }

        public Location Location { get; set; } = new();

        public Priority Priority { get; set; }
        public Status Status { get; set; }

        public DateTime Created { get; set; }

        public virtual ICollection<StatusUpdate> StatusUpdates { get; set; } = new List<StatusUpdate>();

        public virtual ICollection<IncidentPhoto> Photos { get; set; } = new List<IncidentPhoto>();


    }
}
