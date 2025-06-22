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
        public string Name { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty; 

        [JsonPropertyName("userId")] 
        public string? UserId { get; set; } 

        [JsonPropertyName("userName")] 
        public string? UserName { get; set; } = string.Empty; 

        [JsonPropertyName("location")]
        public Location? Location { get; set; } = new(); 

        [JsonPropertyName("priority")]
        public Priority Priority { get; set; } 

        [JsonPropertyName("status")]
        public Status Status { get; set; }  

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

       
        [JsonPropertyName("statusUpdates")] 
        public List<IncidentStatusUpdate>? StatusUpdates { get; set; } = new(); 

        [JsonPropertyName("photoFilePaths")] 
        public List<string>? PhotoFilePaths { get; set; } = new();
    }
}