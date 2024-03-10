namespace SimpleGpio.Models
{
    public class FiringSystemPin
    {
        public int GpioPin { get; set; }
        public string FiringAddress { get; set; }
        public FiringState State { get; set; } = FiringState.Ready;
    }
}