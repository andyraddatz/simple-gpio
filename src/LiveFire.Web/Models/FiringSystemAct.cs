using System.Collections.Generic;

namespace LiveFire.Web.Models
{
    public class FiringSystemAct
    {
        public string? Name { get; set; }
        public int ActNumber { get; set; }
        public FiringState State { get; set; } = FiringState.Ready;
        public IList<FiringCue> FiringCues { get; set; } = new List<FiringCue>();
    }
}