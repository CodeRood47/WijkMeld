using System.ComponentModel.DataAnnotations;
using WijkMeld.API.Entities.Enums;

namespace WijkMeld.API.Controllers.Dto
{
    public class CreateIncidentDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Priority Priority { get; set; }
    }

    public class UpdateIncidentDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class UpdateIncidentStatusDto
    {
        [Required]
        public Status NewStatus { get; set; }
        [Required]
        public string Note { get; set; }
        public Priority? NewPriority { get; set; } // Optioneel
    }
}

