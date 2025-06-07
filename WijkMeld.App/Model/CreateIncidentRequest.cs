using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WijkMeld.App.Model.Enums;

namespace WijkMeld.App.Model
{
    public class CreateIncidentRequest
    {

        public string Name { get; set; }
        public string Description { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public int Priority { get; set; }
    
    }
}
