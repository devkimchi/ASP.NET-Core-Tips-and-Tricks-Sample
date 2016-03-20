using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

using AspNetCoreTipsAndTricksSample.Filters;
using AspNetCoreTipsAndTricksSample.Services;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;
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
        private const string ExceptionsOnStartup = "Startup";
        private const string ExceptionsOnConfigureServices = "ConfigureServices";

        private readonly Dictionary<string, List<Exception>> _exceptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env"><see cref="IHostingEnvironment"/> instance.</param>
        /// <param name="appEnv"><see cref="IApplicationEnvironment"/> instance.</param>
        /// <param name="logger"><see cref="ILoggerFactory"/> instance.</param>
        /// <param name="args">List of arguments from the command line.</param>
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv, ILoggerFactory logger, string[] args = null)
        {
            this._exceptions = new Dictionary<string, List<Exception>>
                                   {
                                       { ExceptionsOnStartup, new List<Exception>() },
                                       { ExceptionsOnConfigureServices, new List<Exception>() },
                                   };

            try
            {
                this.HostingEnvironment = env;
                this.ApplicationEnvironment = appEnv;
                this.LoggerFactory = logger;

                var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables();

                if (args != null && args.Length > 0)
                {
                    builder.AddCommandLine(args);
                }

                this.Configuration = builder.Build();
            }
            catch (Exception ex)
            {
                this._exceptions[ExceptionsOnStartup].Add(ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="IApplicationEnvironment"/> instance.
        /// </summary>
        public IApplicationEnvironment ApplicationEnvironment { get; }

        /// <summary>
        /// Gets the <see cref="IConfigurationRoot"/> instance.
        /// </summary>
        public IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// Gets the <see cref="IHostingEnvironment"/> instance.
        /// </summary>
        public IHostingEnvironment HostingEnvironment { get; }

        /// <summary>
        /// Gets the <see cref="ILoggerFactory"/> instance.
        /// </summary>
        public ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Defines the main entry point of the web application.
        /// </summary>
        /// <param name="args">List of arguments.</param>
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);

        /// <summary>
        /// Configures modules.
        /// </summary>
        /// <param name="app"><see cref="IApplicationBuilder"/> instance.</param>
        /// <param name="env"><see cref="IHostingEnvironment"/> instance.</param>
        /// <param name="logger"><see cref="ILoggerFactory"/> instance.</param>
        /// <remarks>This method gets called by the runtime. Use this method to configure the HTTP request pipeline.</remarks>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory logger)
        {
            var log = logger.CreateLogger<Startup>();
            if (this._exceptions.Any(p => p.Value.Any()))
            {
                app.Run(
                    async context =>
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            context.Response.ContentType = "text/plain";

                            foreach (var ex in this._exceptions)
                            {
                                foreach (var val in ex.Value)
                                {
                                    log.LogError($"{ex.Key}:::{val.Message}");
                                    await context.Response.WriteAsync($"Error on {ex.Key}: {val.Message}").ConfigureAwait(false);
                                }
                            }
                        });
                return;
            }

            try
            {
                logger.AddConsole(this.Configuration.GetSection("Logging"));
                logger.AddDebug();

                app.UseExceptionHandler(
                    builder =>
                        {
                            builder.Run(
                                async context =>
                                    {
                                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                        context.Response.ContentType = "text/html";

                                        var error = context.Features.Get<IExceptionHandlerFeature>();
                                        if (error != null)
                                        {
                                            await context.Response.WriteAsync($"<h1>Error: {error.Error.Message}</h1>").ConfigureAwait(false);
                                        }
                                    });
                        });

                app.UseIISPlatformHandler();

                app.UseDefaultFiles();
                app.UseStaticFiles();

                app.UseMvc();

                app.UseSwaggerGen();
                app.UseSwaggerUi();
            }
            catch (Exception ex)
            {
                app.Run(
                    async context =>
                        {
                            log.LogError($"{ex.Message}");

                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            context.Response.ContentType = "text/plain";
                            await context.Response.WriteAsync(ex.Message).ConfigureAwait(false);
                            await context.Response.WriteAsync(ex.StackTrace).ConfigureAwait(false);
                        });
            }
        }

        /// <summary>
        /// Configures services including dependencies.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> instance.</param>
        /// <remarks>This method gets called by the runtime. Use this method to add services to the container.</remarks>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                this.ConfigureMvc(services, this.LoggerFactory);
                this.ConfigureSwagger(services, this.ApplicationEnvironment);

                return this.ConfigureDependencies(services);
            }
            catch (Exception ex)
            {
                this._exceptions[ExceptionsOnConfigureServices].Add(ex);
                return null;
            }
        }

        private static string GetXmlPath(IApplicationEnvironment appEnv)
        {
            var assembly = typeof(Startup).GetTypeInfo().Assembly;
            var assemblyName = assembly.GetName().Name;

            var path = $@"{appEnv.ApplicationBasePath}\{assemblyName}.xml";
            if (File.Exists(path))
            {
                return path;
            }

            var config = appEnv.Configuration;
            var runtime = $"{appEnv.RuntimeFramework.Identifier.ToLower()}{appEnv.RuntimeFramework.Version.ToString().Replace(".", string.Empty)}";

            path = $@"{appEnv.ApplicationBasePath}\..\..\artifacts\bin\{assemblyName}\{config}\{runtime}\{assemblyName}.xml";
            return path;
        }

        private IServiceProvider ConfigureDependencies(IServiceCollection services)
        {
            // services.AddTransient<IValueService, ValueService>();
            var builder = new ContainerBuilder();

            builder.RegisterType<ValueService>().As<IValueService>();

            builder.Populate(services);

            var container = builder.Build();
            return container.Resolve<IServiceProvider>();
        }

        private void ConfigureMvc(IServiceCollection services, ILoggerFactory logger)
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

            // Setup glogal action filters
            builder.AddMvcOptions(o => { o.Filters.Add(new GlobalActionFilter(logger)); });

            // Setup global exception filters
            builder.AddMvcOptions(o => { o.Filters.Add(new GlobalExceptionFilter(logger)); });
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
                        options.DocumentFilter<SchemaDocumentFilter>();
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
    }
}