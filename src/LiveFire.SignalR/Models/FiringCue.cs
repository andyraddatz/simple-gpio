using System.Collections.Generic;

namespace LiveFire.SignalR.Models
{
    public class FiringCue
    {
        public string FiringAddress { get; set; }
        public int FireAfterSeconds { get; set; }
    }
}