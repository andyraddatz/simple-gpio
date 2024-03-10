using System.Device.Gpio;

namespace LiveFire.Web.Models
{
    public static class Constants
    {
        public static readonly PinValue DefaultFirePinValue = PinValue.Low;
        public static readonly PinValue DefaultOffPinValue = PinValue.High;
        public const int RasPiPinCount = 28;
    }

    public enum FiringState
    {
        Ready,
        Armed,
        Firing,
        Fired
    }
}