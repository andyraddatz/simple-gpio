using LiveFire.SignalR.Models;
using LiveFire.SignalR.Pages;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LiveFire.SignalR.Hubs
{
    public class FireStateHub : Hub
    {
        public async Task ArmPin(string gpioPin)
        {
            if (int.TryParse(gpioPin, out var pin))
            {
                FireStateModel.ArmPin(pin);
                await SendLivePinStates(new List<int> { pin });
            }
        }

        public async Task DisarmPin(string gpioPin)
        {
            if (int.TryParse(gpioPin, out var pin))
            {
                FireStateModel.DisarmPin(pin);
                await SendLivePinStates(new List<int> { pin });
            }
        }

        public async Task FirePins()
        {
            var firingTimers = FireStateModel.FirePinsManually();
            await RunFiringTimers(firingTimers);
        }

        private async Task RunFiringTimers(IEnumerable<FiringTimer> firingTimers)
        {
            foreach (var firingTimer in firingTimers)
            {
                await SendLivePinStates(firingTimer.GpioPins);
                await Task.Delay((int)Math.Ceiling(firingTimer.FiringMs + 100)); // small timer buffer for updating UI
                await SendLivePinStates(firingTimer.GpioPins);
            }
        }
        public async Task ArmAct(string act)
        {
            if (int.TryParse(act, out var actNumber))
            {
                FireStateModel.ArmAct(actNumber);
                await SendLiveActStates(new List<int> { actNumber });
            }
        }

        public async Task DisarmAct(string act)
        {
            if (int.TryParse(act, out var actNumber))
            {
                FireStateModel.DisarmAct(actNumber);
                await SendLiveActStates(new List<int> { actNumber });
            }
        }

        public async Task RunActs()
        {
            var actsToRun = FireStateModel.RunActs();

            foreach (var act in actsToRun.OrderBy(a => a.ActNumber))
            {
                for (int seconds = -10; seconds <= act.FiringCues.Max(fc => fc.FireAfterSeconds); seconds++)
                {
                    Console.WriteLine($"{seconds} seconds");

                    var addressesFired = string.Empty;
                    var firingTimers = FireStateModel.FireActCue(act, seconds);

                    if (firingTimers.Any())
                    {
                        addressesFired = string.Join(", ", firingTimers.SelectMany(t => t.FiringAddresses));
                        
                        // don't wait here, ship UI updates to background thread
                        _ = Task.Run(() => RunFiringTimers(firingTimers)).ConfigureAwait(false);
                    }

                    // wait one second
                    System.Threading.Thread.Sleep(new TimeSpan(0, 0, 1));

                    await SendShowUpdate(seconds, addressesFired);
                }

                // wait a final second
                System.Threading.Thread.Sleep(new TimeSpan(0, 0, 1));

                // notify clients
                act.State = FiringState.Fired;
                await SendLiveActStates(new List<int> { act.ActNumber });
                await SendLivePinStates();
            }
        }

        private async Task SendShowUpdate(int seconds, string addressesFired)
        {
            await Clients.All.SendAsync("ReceiveShowUpdate", new
            {
                seconds,
                addressesFired
            });
        }

        public async Task ResetActs()
        {
            FireStateModel.InitializeActs();
            await SendLiveActStates();
        }

        public async Task ResetPins()
        {
            FireStateModel.InitializeGpioPins();
            await SendLivePinStates();
        }

        public async Task SendLivePinStates(IEnumerable<int> gpioPins = null)
        {
            var pins = FireStateModel.GetViewModelFromLivePinStates();

            var updates = (gpioPins != null)
                ? pins.Where(p => gpioPins.Contains(p.GpioPin))
                : pins;

            await Clients.All.SendAsync("ReceiveLivePinStates", updates);
            Console.WriteLine($"Live pin state SENT for pin(s) {string.Join(", ", updates.Select(p => p.GpioPin))}");
        }
        public async Task SendLiveActStates(IEnumerable<int> actNumbers = null)
        {
            var acts = FireStateModel.GetViewModelFromLiveActStates();

            var updates = (actNumbers != null)
                ? acts.Where(p => actNumbers.Contains(p.ActNumber))
                : acts;

            await Clients.All.SendAsync("ReceiveLiveActStates", updates);
            Console.WriteLine($"Live act state SENT for act(s) {string.Join(", ", updates.Select(p => p.ActNumber))}");
        }
    }
}