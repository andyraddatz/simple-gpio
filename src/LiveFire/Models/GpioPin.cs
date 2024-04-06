using System.Device.Gpio;

namespace SimpleGpio.Models;

public class GpioPin(int pinNumber) : IGpioControllerUser
{
    private static bool HasGpioController => IGpioControllerUser.HasGpioController;
    private GpioController Gpio => (this as IGpioControllerUser).Gpio;
    public int PinNumber { get; } = pinNumber;
    private bool _dummyPinValue;
    public bool PinValue
    {
        get
        {
            if (!HasGpioController) return _dummyPinValue;

            if (!Gpio.IsPinOpen(PinNumber))
                Gpio.OpenPin(PinNumber, PinMode.Input);
            else if (Gpio.GetPinMode(PinNumber) != PinMode.Input)
                Gpio.SetPinMode(PinNumber, PinMode.Input);

            return (bool)Gpio.Read(PinNumber);
        }
        set
        {
            if (!HasGpioController)
            {
                _dummyPinValue = value;
                return;
            }

            if (!Gpio.IsPinOpen(PinNumber))
                Gpio.OpenPin(PinNumber, PinMode.Output);
            else if (Gpio.GetPinMode(PinNumber) != PinMode.Output)
                Gpio.SetPinMode(PinNumber, PinMode.Output);

            Gpio.Write(PinNumber, value);
        }
    }
}
