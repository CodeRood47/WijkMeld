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
    public class IncidentResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public LocationDto Location { get; set; }
        public Priority Priority { get; set; }
        public Status Status { get; set; }
        public DateTime Created { get; set; }

        public Guid? UserId { get; set; }
        public string? UserName { get; set; }

        public List<Guid> StatusUpdateIds { get; set; } = new();
        public List<string> PhotoFilePaths { get; set; } = new();
    }


}

