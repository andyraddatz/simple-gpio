using SimpleGpio.Models;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SimpleGpio.Web
{
    public class IndexModel : PageModel
    {
        private static readonly GpioController _controller;
        private static readonly bool _liveGpioController;
        private static readonly int _pinCount;
        private static PinValue _firePinValue;
        private static PinValue _offPinValue;
        private static bool _timedFiringMode;
        private static int _firingMs;
        public static List<FiringSystemPin> LivePinStates { get; set; }
        static IndexModel()
        {
            // read "Fire" pin value from config
            _firePinValue = Startup.FiringSystemOptions.CurrentValue.FirePinValue;
            _offPinValue = _firePinValue == PinValue.High
                ? PinValue.Low : PinValue.High;

            // read "FiringMs" from config
            _firingMs = Startup.FiringSystemOptions.CurrentValue.FiringMs;

            // default to Raspberry Pi pin count
            _pinCount = Constants.RasPiPinCount;
            try
            {
                _controller = new GpioController();
                _pinCount = _controller.PinCount;
                _liveGpioController = true;
            }
            catch (System.PlatformNotSupportedException)
            {
                // bad hack for windows debugging
                _liveGpioController = false;
            }
            InitializeGpioPins();
        }

        private static void InitializeGpioPins()
        {
            // open all GPIO pins for output
            if (_controller != default)
            {
                foreach (var pin in Startup.FiringSystemOptions.CurrentValue.PinMappings)
                {
                    if (!_controller.IsPinOpen(pin.GpioPin))
                        _controller.OpenPin(pin.GpioPin, PinMode.Output);

                    if (_controller.GetPinMode(pin.GpioPin) != PinMode.Output)
                        _controller.SetPinMode(pin.GpioPin, PinMode.Output);
                }
            }

            WritePinsToGpio(Startup.FiringSystemOptions.CurrentValue.PinMappings);
            ReadLivePinStates();
        }
        private static void WritePinsToGpio(List<FiringSystemPin> pins)
        {
            // reset to ready/off state
            foreach (var pin in pins.Where(p => p.State == FiringState.Ready))
                WritePinValue(pin.GpioPin, _offPinValue);

            FirePins(pins.Where(p => p.State == FiringState.Armed));
        }

        private static void ReadLivePinStates()
        {
            for (var i = 0; i < _pinCount; i++)
            {
                var pin = LivePinStates.Single(p => p.GpioPin == i);

                // read states into static var
                if (_liveGpioController)
                {
                    var currentPinValue = _controller.Read(i);
                    if (currentPinValue == _firePinValue) pin.State = FiringState.Firing;
                    else if (currentPinValue == _offPinValue)
                    {
                        if (pin.State == FiringState.Firing) pin.State = FiringState.Fired;
                    }
                }
            }
        }
        private static void FirePins(IEnumerable<FiringSystemPin> pins)
        {
            // set to fire/on pin state
            Console.WriteLine(
                $"{(_liveGpioController ? "LIVE FIRE MODE: " : "(dry run): ")}Firing pins {string.Join(", ", pins.Select(p => p.GpioPin))}");

            foreach (var pin in pins) WritePinValue(pin.GpioPin, _firePinValue);

            // IF timed fire mode, wait for specified time, then turn off
            if (_timedFiringMode)
            {
                // create the firing timer
                var firingTimer = new System.Timers.Timer(_firingMs);

                // set the timer to run once
                firingTimer.AutoReset = false;

                // turn pins off after timer has elapsed
                firingTimer.Elapsed += (src, args) =>
                {
                    foreach (var pin in pins) WritePinValue(pin.GpioPin, _offPinValue);
                    Console.WriteLine($"Pins fired for {_firingMs} milliseconds");
                    firingTimer.Stop();
                    firingTimer.Dispose();
                };

                // start timer
                firingTimer.Enabled = true;
            }
        }

        private static void WritePinValue(int gpioPin, PinValue pinValue)
        {
            if (_liveGpioController) _controller.Write(gpioPin, pinValue);
            Console.WriteLine($"GPIO pin {gpioPin} set to {pinValue.ToString()}");
        }
        private void SetViewModelFromLivePinStates()
        {
            ReadLivePinStates();
            throw new NotImplementedException();
        }

        public void OnGet()
        {
            SetViewModelFromLivePinStates();
        }

        public ActionResult OnPostResetAllPins()
        {
            InitializeGpioPins();
            return RedirectToPage("Index");
        }
        [BindProperty]
        public List<PinViewModel> Pins { get; set; }
        public ActionResult OnPost()
        {
            ReadLivePinStates();
            List<FiringSystemPin> changedPinStates = new();

            // because of disable behavior on inputs, only new pinsettings will be sent from form
            // https://stackoverflow.com/questions/7357256/disabled-form-inputs-do-not-appear-in-the-request
            foreach (var pinState in LivePinStates)
            {
                var viewModelPin = Pins.Single(p => p.GpioPin == pinState.GpioPin);

                // if user passed in a new state
                if (viewModelPin.State != pinState.State)
                {
                    changedPinStates.Add(new FiringSystemPin
                    {
                        GpioPin = pinState.GpioPin,
                        FiringAddress = pinState.FiringAddress,
                        State = viewModelPin.State
                    });
                }
            }

            WritePinsToGpio(changedPinStates);
            return RedirectToPage("Index");
        }
    }
}