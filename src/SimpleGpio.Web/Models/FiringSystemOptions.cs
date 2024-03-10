using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Device.Gpio;

namespace SimpleGpio.Models
{
    public class FiringSystemOptions
    {
        [Required]
        public PinValue FirePinValue { get; set; }
        public bool TimedFireMode { get; set; } = true;
        public int FiringMs { get; set; } = Constants.DefaultFiringMs;
        public List<FiringSystemPin> PinMappings { get; set; }
    }
}