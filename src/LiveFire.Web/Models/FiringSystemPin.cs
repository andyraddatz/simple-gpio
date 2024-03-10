using System.Collections.Generic;

namespace LiveFire.Web.Models
{
    public class FiringSystemPin
    {
        public PinMapping PinMapping { get; set; }
        public FiringState State { get; set; } = FiringState.Ready;
    }
}