using System.Device.Gpio;
using LiveFire.Web.Models;
using Microsoft.Extensions.Options;

namespace LiveFire.Web.Data
{
    public class GpioService
    {
        private readonly GpioController? _controller;
        private readonly int _pinCount;
        private readonly PinValue _firePinValue;
        private readonly PinValue _offPinValue;
        private readonly FiringSystemOptions? _options;
        public event Func<string, bool, Task>? NotifyCueStateChange;
        public List<PinMapping> PinMappings { get; set; }

        public GpioService(IOptions<FiringSystemOptions> options)
        {
            _options = options.Value;

            // read "Fire" pin value from config
            _firePinValue = _options.FirePinValue;
            _offPinValue = _firePinValue == PinValue.High
                ? PinValue.Low : PinValue.High;

            // default to Raspberry Pi pin count
            _pinCount = Constants.RasPiPinCount;
            try
            {
                _controller = new GpioController();
                _pinCount = _controller.PinCount;
            }
            catch (System.PlatformNotSupportedException) { } // no GPIO pins available

            PinMappings = new();

            for (int pin = 0; pin < _pinCount; pin++)
            {
                var configuredPinMapping = _options?.PinMappings?.SingleOrDefault(pm => pm.GpioPin == pin);
                var firingAddress = configuredPinMapping?.FiringAddress ?? $"GPIO #{pin}";

                PinMappings.Add(new PinMapping(pin, firingAddress));

                // open all GPIO pins for output
                if (_controller is not null)
                {
                    // open GPIO pin
                    if (!_controller.IsPinOpen(pin))
                        _controller.OpenPin(pin, PinMode.Output);

                    // set output mode
                    if (_controller.GetPinMode(pin) != PinMode.Output)
                        _controller.SetPinMode(pin, PinMode.Output);

                    // wire up state change events to memory cache
                    _controller.RegisterCallbackForPinValueChangedEvent(
                        pin,
                        PinEventTypes.Falling | PinEventTypes.Rising,
                        OnPinChangeEvent);
                }

                // ensure firing state is off
                WritePinValue(pin, _offPinValue);
            }
        }

        private void OnPinChangeEvent(object sender, PinValueChangedEventArgs args)
        {
            if (NotifyCueStateChange is null) return;
            if (TryGetFiringAddress(args.PinNumber, out var address))
            {
                var isFiring = args.ChangeType == PinEventTypes.Rising
                    ? _firePinValue == PinValue.High
                    : _firePinValue == PinValue.Low;

                NotifyCueStateChange.Invoke(address, isFiring).Wait();
            }
        }

        public bool TryGetFiringAddress(int pin, out string address)
        {
            address = string.Empty;

            var map = PinMappings.SingleOrDefault(pm => pm.GpioPin == pin);
            if (map is null) return false;

            address = map.FiringAddress;
            return true;
        }

        public bool TryGetGpioPin(string address, out int pin)
        {
            pin = default;

            var map = PinMappings.SingleOrDefault(pm => pm.FiringAddress == address);
            if (map is null) return false;

            pin = map.GpioPin;
            return true;
        }

        private void WritePinValue(int gpioPin, PinValue pinValue)
        {
            if (_controller is not null) _controller.Write(gpioPin, pinValue);
            Console.WriteLine($"GPIO pin {gpioPin} set to {pinValue.ToString()}");
        }

        public void SetCueState(string address, bool isFiring)
        {
            if (TryGetGpioPin(address, out var pin))
            {
                WritePinValue(pin, isFiring ? _firePinValue : _offPinValue);
            }
        }

        public bool IsFiring(string address)
        {
            if (TryGetGpioPin(address, out var pin))
            {
                if (_controller is not null)
                {
                    var pinValue = _controller.Read(pin);
                    return _firePinValue == pinValue;
                }
            }
            return false;
        }
    }
}