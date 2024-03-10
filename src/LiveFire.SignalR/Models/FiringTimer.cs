using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

namespace LiveFire.SignalR.Models
{
    public class FiringTimer : Timer
    {
        private Stopwatch Stopwatch { get; set; }
        public FiringTimer(int gpioPin, string firingAddress, double interval) : this(new List<int> { gpioPin }, new List<string> { firingAddress }, interval) { }
        public FiringTimer(IEnumerable<int> gpioPins, IEnumerable<string> firingAddresses, double interval) : base(interval)
        {
            GpioPins = gpioPins;
            FiringAddresses = firingAddresses;
            FiringMs = interval;
            Stopwatch = new Stopwatch();
            AutoReset = false;
        }
        public IEnumerable<int> GpioPins { get; }
        public IEnumerable<string> FiringAddresses { get; }
        public double FiringMs { get; }
        public new void Start() {
            Stopwatch.Start();
            base.Start();
        }
        public new void Stop() {
            Stopwatch.Stop();
            Console.WriteLine($"Pin(s) {string.Join(", ", GpioPins)} fired for {Stopwatch.ElapsedMilliseconds} milliseconds");  
        }
    }
}