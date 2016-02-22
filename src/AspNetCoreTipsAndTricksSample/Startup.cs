using System;
using System.Reflection;

using AspNetCoreTipsAndTricksSample.Services;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

using Swashbuckle.SwaggerGen;
using Swashbuckle.SwaggerGen.XmlComments;

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
        /// <param name="env"><see cref="IHostingEnvironment"/> instance.</param>
        /// <param name="appEnv"><see cref="IApplicationEnvironment"/> instance.</param>
        /// <param name="args">List of arguments from the command line.</param>
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv, string[] args = null)
        {
            this.HostingEnvironment = env;
            this.ApplicationEnvironment = appEnv;

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
        /// Gets the <see cref="IHostingEnvironment"/> instance.
        /// </summary>
        public IHostingEnvironment HostingEnvironment { get; }

        /// <summary>
        /// Gets the <see cref="IApplicationEnvironment"/> instance.
        /// </summary>
        public IApplicationEnvironment ApplicationEnvironment { get; }

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
            this.ConfigureMvc(services);
            this.ConfigureSwagger(services, this.ApplicationEnvironment);
            return this.ConfigureDependencies(services);
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

            app.UseSwaggerGen();
            app.UseSwaggerUi();
        }

        /// <summary>
        /// Defines the main entry point of the web application.
        /// </summary>
        /// <param name="args">List of arguments.</param>
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);

        private void ConfigureMvc(IServiceCollection services)
        {
            var builder = services.AddMvc();

            builder.AddJsonOptions(
                o =>
                    {
                        o.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                        o.SerializerSettings.Converters.Add(new StringEnumConverter());
                        o.SerializerSettings.Formatting = Formatting.Indented;
                        o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                        o.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                    });
        }

        private void ConfigureSwagger(IServiceCollection services, IApplicationEnvironment appEnv)
        {
            services.AddSwaggerGen();

            services.ConfigureSwaggerDocument(
                options =>
                {
                    options.SingleApiVersion(new Info() { Version = "v1", Title = "Swagger UI" });
                    options.IgnoreObsoleteActions = true;
                    options.OperationFilter(new ApplyXmlActionComments(GetXmlPath(appEnv)));
                });

            services.ConfigureSwaggerSchema(
                options =>
                {
                    options.DescribeAllEnumsAsStrings = true;
                    options.IgnoreObsoleteProperties = true;
                    options.CustomSchemaIds(type => type.FriendlyId(true));
                    options.ModelFilter(new ApplyXmlTypeComments(GetXmlPath(appEnv)));
                });
        }

        private IServiceProvider ConfigureDependencies(IServiceCollection services)
        {
            //services.AddTransient<IValueService, ValueService>();

            var builder = new ContainerBuilder();

            builder.RegisterType<ValueService>().As<IValueService>();

            builder.Populate(services);

            var container = builder.Build();
            return container.Resolve<IServiceProvider>();
        }
        private static string GetXmlPath(IApplicationEnvironment appEnv)
        {
            var assembly = typeof(Startup).GetTypeInfo().Assembly;
            var buildConfig = "Release";

#if DEBUG
            buildConfig = "Debug";
#endif

            var path = $@"{appEnv.ApplicationBasePath}\..\..\artifacts\bin\{assembly.GetName().Name}\{buildConfig}\dnx451\{assembly.GetName().Name}.xml";
            return path;
        }
    }
}