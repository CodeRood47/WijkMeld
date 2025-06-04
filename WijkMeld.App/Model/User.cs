using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WijkMeld.App.Model.Enums;

namespace WijkMeld.App.Model
{
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }

        public string PasswordHash { get; set; } 
        public string Email { get; set; }  

        public UserRole Role { get; set; }

        public List<Incident> Incidents { get; set; } = new List<Incident>();

    }
}
