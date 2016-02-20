using System;

using AspNetCoreTipsAndTricksSample.Services;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace AspNetCoreTipsAndTricksSample
{
    /// <summary>
    /// This represents the main entry point of the web application.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="args">List of arguments from the command line.</param>
        public Startup(string[] args = null)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                              .AddJsonFile("appsettings.json")
                              .AddEnvironmentVariables();

            if (args != null && args.Length > 0)
            {
                builder.AddCommandLine(args);
            }

            this.Configuration = builder.Build();
        }

        /// <summary>
        /// Gets the <see cref="IConfigurationRoot"/> instance.
        /// </summary>
        public IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// Configures services including dependencies.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> instance.</param>
        /// <remarks>This method gets called by the runtime. Use this method to add services to the container.</remarks>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            //services.AddTransient<IValueService, ValueService>();

            var builder = new ContainerBuilder();

            builder.RegisterType<ValueService>().As<IValueService>();

            builder.Populate(services);

            var container = builder.Build();
            return container.Resolve<IServiceProvider>();
        }

        /// <summary>
        /// Configures modules.
        /// </summary>
        /// <param name="app"><see cref="IApplicationBuilder"/> instance.</param>
        /// <param name="env"><see cref="IHostingEnvironment"/> instance.</param>
        /// <param name="logger"><see cref="ILoggerFactory"/> instance.</param>
        /// <remarks>This method gets called by the runtime. Use this method to configure the HTTP request pipeline.</remarks>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory logger)
        {
            logger.AddConsole(this.Configuration.GetSection("Logging"));
            logger.AddDebug();

            app.UseIISPlatformHandler();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseMvc();
        }

        /// <summary>
        /// Defines the main entry point of the web application.
        /// </summary>
        /// <param name="args">List of arguments.</param>
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}