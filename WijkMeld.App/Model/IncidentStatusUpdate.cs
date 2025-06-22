using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WijkMeld.App.Model.Enums;


namespace WijkMeld.App.Model
   {
       public class IncidentStatusUpdate
       {
           public Status? NewStatus { get; set; }
           public string? Note { get; set; }
           public Priority? NewPriority { get; set; }
       }
   }


