using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WijkMeld.App.Model
{
    public class Notification
    {
        public Guid Id { get; set; }
        public User Receiver { get; set; }
        public Guid IncidentId { get; set; }
        public string Message {  get; set; }   
        public DateTime SendDate { get; set; }
        public bool Read {  get; set; }

    }
}
