using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WijkMeld.API.Entities.Enums; // Zorg dat deze using aanwezig is

namespace WijkMeld.API.Entities
{
    public class StatusUpdate
    {
        [Key]
        public Guid Id { get; set; }

        // Foreign Key naar Incident
        public Guid IncidentId { get; set; }
        [ForeignKey(nameof(IncidentId))]
        public Incident Incident { get; set; } = default!; 

      
        //public Status? OldStatus { get; set; } 
        //public Status? NewStatus { get; set; } 

        //public Priority? OldPrio { get; set; } 
        public Priority? NewPrio { get; set; } 

     
        public Guid ChangedById { get; set; } 
        [ForeignKey(nameof(ChangedById))]
        public User ChangedBy { get; set; } = default!; 

        public DateTime Date { get; set; }

        public string? Note { get; set; }
    }
}
