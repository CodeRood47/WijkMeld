using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WijkMeld.App.Model.Enums;

namespace WijkMeld.App.Model
{
    public class StatusUpdate
    {
        public Guid Id { get; set; }
        
        // hier moet misschien nog incident ID bij.

        public Status NewStatus { get; set; }

        public User ChangedBy { get; set; }

        public DateTime Date {  get; set; }

        public string Note { get; set; }

        public Priority? NewPrio { get; set; }
    }
}
