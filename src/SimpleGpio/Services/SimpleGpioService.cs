using SimpleGpio.Models;

namespace SimpleGpio.Services;

internal class SimpleGpioService
{
    private readonly bool _hasGpioController;
    public IEnumerable<GpioPin> Pins { get; }
    public SimpleGpioService(bool hasGpioController, int pinCount)
    {
        _hasGpioController = hasGpioController;
        var pins = new List<GpioPin>();
        for (var p = 1; p <= pinCount; p++)
            pins.Add(new GpioPin(_hasGpioController, p));

        Pins = pins;
    }

}