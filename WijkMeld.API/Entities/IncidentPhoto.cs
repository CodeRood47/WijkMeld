using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WijkMeld.API.Entities
{
    public class IncidentPhoto
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string FilePath { get; set; } = null!;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("Incident")]
        public Guid IncidentId { get; set; }
        public Incident Incident { get; set; } = null!;
    }
}
