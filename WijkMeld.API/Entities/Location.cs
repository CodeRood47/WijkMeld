using System.ComponentModel.DataAnnotations;

namespace WijkMeld.API.Entities
{
    public class Location
    {

        public double Lat { get; set; }
        public double Long { get; set; }
        public string? Address { get; set; }

    }
}
