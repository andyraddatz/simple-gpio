using System.Device.Gpio;

namespace LiveFire.Web.Models;
public class FiringSystemOptions
{
    public PinValue FirePinValue { get; set; }
    public IEnumerable<PinMapping>? PinMappings { get; set; }
    public IEnumerable<FiringSystemAct>? Acts { get; set; }
}