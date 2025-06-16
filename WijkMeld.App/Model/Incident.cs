using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WijkMeld.App.Model.Enums;

namespace WijkMeld.App.Model
{
    public class Incident
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public User? user { get; set; }

        [JsonPropertyName("userName")] 
        public string UserName { get; set; }

        public Location Location { get; set; }

        public string? PictureUrl { get; set; }

        public Priority Prio { get; set; }

        [JsonPropertyName("created")] // Matcht met de "created" in de JSON-respons
        public DateTime date { get; set; } 

        [JsonPropertyName("status")] 
        public Status Status { get; set; }

        public List<StatusUpdate>? History { get; set; }
    }
}
