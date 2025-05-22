using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WijkMeld.API.Entities
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User Receiver { get; set; }
        public Guid IncidentId { get; set; }

        [ForeignKey("IncidentId")]
        public Incident Incident { get; set; }
        public string Message { get; set; }
        public DateTime SendDate { get; set; }
        public bool Read { get; set; }
    }
}
