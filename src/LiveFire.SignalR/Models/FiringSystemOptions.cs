using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Device.Gpio;

namespace LiveFire.SignalR.Models
{
    public class FiringSystemOptions
    {
        [Required]
        public PinValue FirePinValue { get; set; }
        public bool TimedFiringMode { get; set; } = true;
        public bool SequentialFiringMode { get; set; } = true;
        public int FiringMs { get; set; } = Constants.DefaultFiringMs;
        public IList<FiringSystemPin> PinMappings { get; set; }
        public IList<FiringSystemAct> Acts { get; set; }
        public string AdminUserCode { get; set; }
    }
}