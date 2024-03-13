using System.Device.Gpio;
using SimpleGpio.Services;

namespace SimpleGpio.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        private static bool _hasGpioController;
        private static int _pinCount;
        public static WebApplicationBuilder AddSimpleGpio(this WebApplicationBuilder builder)
        {
            try
            {
                // establish GPIO controller
                using var controller = new GpioController();
                _pinCount = controller.PinCount;
                _hasGpioController = true;
            }
            catch (PlatformNotSupportedException)
            {
                _pinCount = builder.Configuration.GetValue("SimpleGpio:DefaultPinCount", 0);
                _hasGpioController = false;
            }
            finally
            {
                _ = builder.Services.AddSingleton(new SimpleGpioService(_hasGpioController, _pinCount));
            }

            return builder;
        }
    }
}