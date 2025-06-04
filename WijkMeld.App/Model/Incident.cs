using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public Location Location { get; set; }

        public string? PictureUrl { get; set; }

        public Priority Prio { get; set; }

        public DateTime date { get; set; }

        public List<StatusUpdate>? History { get; set; }
    }
}
