namespace LiveFire.SignalR.Models
{
    public class PinViewModel
    {
        public int GpioPin { get; set; }
        public FiringState FiringState { get; set; }
        public bool FiringSwitch { get; set; }
        public string DisplayName { get; set; }
    }
}