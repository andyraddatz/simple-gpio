using System.Device.Gpio;

namespace SimpleGpio.Models;

public interface IGpioControllerUser
{
    private static readonly GpioController _controller;
    public GpioController Gpio => _controller;
    public static bool HasGpioController { get; }
    static IGpioControllerUser()
    {
        // establish GPIO controller
        _controller = new GpioController();
        try
        {
            // this will exception if the platform does not support GPIO
            _ = _controller.PinCount;
            HasGpioController = true;
        }
        catch (PlatformNotSupportedException e)
        {
            throw;
            Console.WriteLine(e.Message);
            HasGpioController = false;
        }
    }
}