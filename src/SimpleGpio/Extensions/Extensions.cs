using System.Device.Gpio;
using SimpleGpio.Services;

namespace SimpleGpio.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder AddSimpleGpio(this WebApplicationBuilder builder)
        {

            // // establish GPIO controller
            // var _controller = new GpioController();
            // try
            // {
            //     // this will exception if the platform does not support GPIO
            //     _ = controller.PinCount;
            //     HasGpioController = true;
            // }
            // catch (PlatformNotSupportedException e)
            // {

            //     HasGpioController = false;
            // }
            _ = builder.Services.AddSingleton<SimpleGpioService>();
            return builder;
        }

        public static WebApplication UseSimpleGpio(this WebApplication app)
        {
            app.Logger.LogInformation("SimpleGpio middleware registered.");
            return app;
        }
    }
}