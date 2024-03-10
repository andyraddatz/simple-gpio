using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LiveFire.SignalR.Models;
using System.Device.Gpio;
using System.Diagnostics;
using System.Timers;

namespace LiveFire.SignalR.Pages
{
    public class FireStateModel : PageModel
    {
        private static readonly GpioController _controller;
        private static readonly bool _liveGpioController;
        private static readonly int _pinCount;
        private static PinValue _firePinValue;
        private static PinValue _offPinValue;
        private static bool _timedFiringMode;
        private static bool _sequentialFiringMode;
        private static int _firingMs;
        private static string _adminUserCode;
        public static List<FiringSystemPin> LivePinStates { get; set; } = new List<FiringSystemPin>();
        public static List<FiringSystemAct> LiveActStates { get; set; } = new List<FiringSystemAct>();
        static FireStateModel()
        {
            var options = Startup.FiringSystemOptionsAccessor.CurrentValue;
            // read "Fire" pin value from config
            _firePinValue = options.FirePinValue;
            _offPinValue = _firePinValue == PinValue.High
                ? PinValue.Low : PinValue.High;

            _timedFiringMode = options.TimedFiringMode;
            _sequentialFiringMode = options.SequentialFiringMode;

            // read "FiringMs" from config
            _firingMs = options.FiringMs;

            // TODO
            _adminUserCode = options.AdminUserCode;

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
            InitializeActs();
        }

        internal static void ArmAct(int actNumber)
        {
            var act = LiveActStates.Single(a => a.ActNumber == actNumber);
            if (act.State == FiringState.Ready)
            {
                act.State = FiringState.Armed;
                Console.WriteLine($"Act {act.ActNumber} ARMED");
            }
        }

        internal static void DisarmAct(int actNumber)
        {
            var act = LiveActStates.Single(a => a.ActNumber == actNumber);
            if (act.State == FiringState.Armed)
            {
                act.State = FiringState.Ready;
                Console.WriteLine($"Act {act.ActNumber} DIS-ARMED");
            }
        }

        internal static void ArmPin(int gpioPin)
        {
            var pin = LivePinStates.Single(p => p.GpioPinNumber == gpioPin);
            if (pin.State == FiringState.Ready)
            {
                pin.State = FiringState.Armed;
                Console.WriteLine($"Pin {pin.GpioPinNumber} ARMED");
            }
        }
        internal static void DisarmPin(int gpioPin)
        {
            var pin = LivePinStates.Single(p => p.GpioPinNumber == gpioPin);
            if (pin.State == FiringState.Armed)
            {
                pin.State = FiringState.Ready;
                Console.WriteLine($"Pin {pin.GpioPinNumber} DIS-ARMED");
            }
        }
        internal static void InitializeActs()
        {
            LiveActStates = new List<FiringSystemAct>();
            // load acts up
            var initialStates = Startup.FiringSystemOptionsAccessor.CurrentValue.Acts;
            if (initialStates == null) return;

            foreach (var act in initialStates)
            {
                LiveActStates.Add(new FiringSystemAct
                {
                    ActNumber = act.ActNumber,
                    Name = act?.Name ?? $"Act {act.ActNumber}",
                    State = act?.State ?? FiringState.Ready,
                    FiringCues = act.FiringCues.ToList()
                });
            }
        }
        internal static void InitializeGpioPins()
        {
            var initialStates = new List<FiringSystemPin>();
            for (int pin = 0; pin < _pinCount; pin++)
            {
                var configuredPinMapping = Startup.FiringSystemOptionsAccessor.CurrentValue.PinMappings.SingleOrDefault(pm => pm.GpioPinNumber == pin);
                var initialState = configuredPinMapping?.State ?? FiringState.Ready;
                var initialName = configuredPinMapping?.FiringAddress ?? $"Default pin {pin}";
                var userCodes = configuredPinMapping?.UserCodes ?? new List<string> { _adminUserCode };

                initialStates.Add(new FiringSystemPin
                {
                    GpioPinNumber = pin,
                    FiringAddress = initialName,
                    State = initialState,
                    UserCodes = userCodes
                });

                // open all GPIO pins for output
                if (_liveGpioController)
                {
                    if (!_controller.IsPinOpen(pin))
                        _controller.OpenPin(pin, PinMode.Output);

                    if (_controller.GetPinMode(pin) != PinMode.Output)
                        _controller.SetPinMode(pin, PinMode.Output);
                }
            }

            WritePinStatesToGpio(initialStates);
            LivePinStates = initialStates.ToList();
        }
        private static void WritePinStateToGpio(FiringSystemPin pin) => WritePinStatesToGpio(new List<FiringSystemPin> { pin });
        private static void WritePinStatesToGpio(IEnumerable<FiringSystemPin> pins)
        {
            // reset to ready/off state
            foreach (var pin in pins)
            {
                switch (pin.State)
                {
                    case FiringState.Firing:
                        WritePinValue(pin.GpioPinNumber, _firePinValue);
                        break;

                    case FiringState.Ready:
                    case FiringState.Armed:
                    case FiringState.Fired:
                    default:
                        WritePinValue(pin.GpioPinNumber, _offPinValue);
                        break;
                }
            }
        }

        public static List<PinViewModel> Pins { get; set; }
        public static List<ActViewModel> Acts { get; set; }

        internal static IEnumerable<FiringSystemAct> RunActs()
        {
            var actsToRun = LiveActStates.Where(p => p.State == FiringState.Armed);
            if (!actsToRun.Any()) return new List<FiringSystemAct>();
            Console.WriteLine($"{(_liveGpioController ? "LIVE FIRE MODE: " : "(dry run mode): ")}Firing ACT(s) {string.Join(", ", actsToRun.Select(a => a.ActNumber))}");
            return actsToRun;
        }
        internal static IEnumerable<FiringTimer> FirePinsManually()
        {
            var pinsToFire = LivePinStates.Where(p => p.State == FiringState.Armed);
            if (!pinsToFire.Any()) return new List<FiringTimer>();
            Console.WriteLine($"{(_liveGpioController ? "LIVE FIRE MODE: " : "(dry run mode): ")}Firing pin(s) {string.Join(", ", pinsToFire.Select(p => p.GpioPinNumber))} {(_sequentialFiringMode ? "sequentially" : "at once")}");

            return DoTimedFire(pinsToFire, _firingMs);
        }
        public static IEnumerable<FiringTimer> FireActCue(FiringSystemAct act, int cueSeconds)
        {
            var cues = act.FiringCues.Where(fc => fc.FireAfterSeconds <= cueSeconds);
            var firingTimers = new List<FiringTimer>();
            foreach (var cue in cues)
            {
                var pin = LivePinStates.SingleOrDefault(s =>
                    s.FiringAddress == cue.FiringAddress
                    && s.State != FiringState.Fired
                    && s.State != FiringState.Firing);

                if (!(pin is null)) firingTimers.Add(DoTimedFire(pin, _firingMs));
            }
            return firingTimers;
        }
        private static FiringTimer DoTimedFire(FiringSystemPin pin, int firingMs) => DoTimedFire(new List<FiringSystemPin> { pin }, firingMs).SingleOrDefault();
        private static IEnumerable<FiringTimer> DoTimedFire(IEnumerable<FiringSystemPin> pins, int firingMs)
        {
            var firingTimers = new List<FiringTimer>();
            foreach (var pin in pins.OrderBy(p => p.FiringAddress))
            {
                // create the firing timer set to run once
                var firingTimer = new FiringTimer(pin.GpioPinNumber, pin.FiringAddress, firingMs);

                // set timer to turn pins off after timer has elapsed
                firingTimer.Elapsed += (src, args) =>
                {
                    var firedPins = LivePinStates.Where(p => (src as FiringTimer).GpioPins.Contains(p.GpioPinNumber));
                    foreach (var pin in firedPins) pin.State = FiringState.Fired;
                    WritePinStatesToGpio(firedPins);
                    firingTimer.Stop();
                };

                // actually fire the pin
                pin.State = FiringState.Firing;
                WritePinStateToGpio(pin);

                // start timer
                firingTimer.Start();
                firingTimers.Add(firingTimer);
            }
            return firingTimers;
        }

        private static void WritePinValue(int gpioPin, PinValue pinValue)
        {
            if (_liveGpioController) _controller.Write(gpioPin, pinValue);
            Console.WriteLine($"GPIO pin {gpioPin} set to {pinValue.ToString()}");
        }
        public string UserCode { get; set; }

        // initial page load
        public void OnGet(string userCode = null)
        {
            UserCode = userCode;
            Pins = GetViewModelFromLivePinStates();
            Acts = GetViewModelFromLiveActStates();
        }

        public static List<ActViewModel> GetViewModelFromLiveActStates() =>
            LiveActStates.Select(a => new ActViewModel
            {
                ActNumber = a.ActNumber,
                DisplayName = a.Name,
                FiringState = a.State,
                FiringSwitch = a.State != FiringState.Ready
            }).ToList();

        public static List<PinViewModel> GetViewModelFromLivePinStates() =>
            LivePinStates
                .Select(p => new PinViewModel
                {
                    GpioPin = p.GpioPinNumber,
                    DisplayName = p.FiringAddress,
                    FiringState = p.State,
                    FiringSwitch = p.State != FiringState.Ready
                }).ToList();
    }
}