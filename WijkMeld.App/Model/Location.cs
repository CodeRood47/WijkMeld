using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WijkMeld.App.Model
{
    public class Location
    {
        [JsonPropertyName("lat")]
        public double lat { get; set; }
        [JsonPropertyName("long")]
        public double lng { get; set; }
        [JsonPropertyName("address")]
        public string Address { get; set; }
    }
}
