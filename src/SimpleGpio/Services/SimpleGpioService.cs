using SimpleGpio.Models;

namespace SimpleGpio.Services;

internal class SimpleGpioService : IGpioControllerUser
{
    public IEnumerable<GpioPin> Pins { get; }
    public SimpleGpioService(IConfiguration configuration)
    {
        var pinCount = IGpioControllerUser.HasGpioController
            ? (this as IGpioControllerUser).Gpio.PinCount
            : configuration.GetValue("SimpleGpio:DefaultPinCount", 0);
            
        var pins = new List<GpioPin>();
        for (var p = 1; p <= pinCount; p++)
            pins.Add(new GpioPin(p));

        Pins = pins;
    }

}