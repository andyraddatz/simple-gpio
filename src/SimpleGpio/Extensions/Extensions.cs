using System.Device.Gpio;
using SimpleGpio.Services;

namespace SimpleGpio.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder AddSimpleGpio(this WebApplicationBuilder builder)
        {
            _ = builder.Services.AddSingleton<SimpleGpioService>();
            return builder;
        }
    }
}