namespace LiveFire.SignalR.Models
{
    public class ActViewModel
    {
        public int ActNumber { get; set; }
        public FiringState FiringState { get; set; }
        public bool FiringSwitch { get; set; }
        public string DisplayName { get; set; }
    }
}