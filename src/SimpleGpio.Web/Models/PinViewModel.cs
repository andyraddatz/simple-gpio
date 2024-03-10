namespace SimpleGpio.Models
{
    public class PinViewModel
    {
        public int GpioPin { get; set; }
        public FiringState State { get; set; }
        public string DisplayName { get; set; }
    }
}