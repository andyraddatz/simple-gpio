using System.Device.Gpio;

namespace SimpleGpio.Models;

public class GpioPin(bool hasGpioController, int pinNumber)
{
    private readonly bool _hasGpioController = hasGpioController;
    private bool _dummyPinValue;

    public int PinNumber { get; } = pinNumber;
    public bool PinValue
    {
        get
        {
            if (!_hasGpioController) return _dummyPinValue;

            using var controller = new GpioController();
            
            if (!controller.IsPinOpen(PinNumber))
                controller.OpenPin(PinNumber, PinMode.Input);
            else if (controller.GetPinMode(PinNumber) != PinMode.Input)
                controller.SetPinMode(PinNumber, PinMode.Input);

            return (bool)controller.Read(PinNumber);
        }
        set
        {
            if (!_hasGpioController)
            {
                _dummyPinValue = value;
                return;
            }

            using var controller = new GpioController();

            if (!controller.IsPinOpen(PinNumber))
                controller.OpenPin(PinNumber, PinMode.Output);
            else if (controller.GetPinMode(PinNumber) != PinMode.Output)
                controller.SetPinMode(PinNumber, PinMode.Output);

            controller.Write(PinNumber, value);
        }
    }
}
