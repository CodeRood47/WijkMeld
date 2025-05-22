using System.ComponentModel.DataAnnotations;
using WijkMeld.API.Entities.Enums;

namespace WijkMeld.API.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }

        public UserRole Role { get; set; }

        public List<Incident> Incidents { get; set; } = new List<Incident>();
    }
}
