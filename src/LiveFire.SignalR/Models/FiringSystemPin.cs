using System.Collections.Generic;

namespace LiveFire.SignalR.Models
{
    public class FiringSystemPin
    {
        public int GpioPinNumber { get; set; }
        public string FiringAddress { get; set; }
        public FiringState State { get; set; } = FiringState.Ready;
        public IEnumerable<string> UserCodes { get; set; } = new List<string>();
    }
}