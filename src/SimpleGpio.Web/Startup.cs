using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SimpleGpio.Models;

namespace SimpleGpio.Web
{
    public class Startup
    {
        // public static IConfiguration StaticConfig { get; set; }
        public static IConfiguration Configuration { get; set; }
        public static IOptionsMonitor<FiringSystemOptions> FiringSystemOptions { get; set; }

        public Startup(IConfiguration configuration)
        {
            // StaticConfig = configuration;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {            
            services.Configure<FiringSystemOptions>(Configuration.GetSection("FiringSystemOptions"));

            services
                .AddRazorPages()
                .AddRazorRuntimeCompilation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptionsMonitor<FiringSystemOptions> optionsAccessor)
        {
            FiringSystemOptions = optionsAccessor;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
