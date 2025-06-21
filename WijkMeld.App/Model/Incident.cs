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

        [JsonPropertyName("userId")]
        public Guid? UserId { get; set; }

        [JsonPropertyName("userName")] 
        public string? UserName { get; set; }

        public Location Location { get; set; }


        public Priority Prio { get; set; }

        [JsonPropertyName("created")] 
        public DateTime date { get; set; } 

        [JsonPropertyName("status")] 
        public Status Status { get; set; }

        [JsonPropertyName("history")]
        public List<StatusUpdate>? History { get; set; }

        [JsonPropertyName("photoFilePaths")]
        public List<string>? PhotoFilePaths { get; set; } = new();
    }
}
