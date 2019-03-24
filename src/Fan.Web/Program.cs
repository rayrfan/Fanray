using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;

namespace Fan.Web
{
    public class Program
    {
        /// <summary>
        /// The entry point for the entire program.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            // Setting values are being overriden in the order of how builder adds them.
            // For example AddEnvironmentVariables() will enable Azure App settings to override what is
            // in appsettings.Production.json which overrides appsettings.json.  
            // For envrionments that don't have ASPNETCORE_ENVIRONMENT set, it gets Production.
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            // We always log to Application Insights, the key is from either appsettings.json or Azure App Service > App settings
            Log.Logger = new LoggerConfiguration()
               .ReadFrom.Configuration(configuration)
               .Enrich.FromLogContext()
               .WriteTo.ApplicationInsights(configuration.GetValue<string>("ApplicationInsights:InstrumentationKey"),
                        TelemetryConverter.Traces, Serilog.Events.LogEventLevel.Information)
               .CreateLogger();

            try
            {
                Log.Information("Starting web host");

                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Returns a <see cref="IWebHostBuilder"/>.
        /// </summary>
        /// <param name="args"></param>
        /// <remarks>
        /// It's built with <see href="https://github.com/aspnet/MetaPackages/blob/dev/src/Microsoft.AspNetCore/WebHost.cs#L148">CreateDefaultBuilder</see>
        /// it loads confiuration from these providers in this order:
        /// - appsettings.json (optional)
        /// - appsettings.{env.EnvironmentName}.json (optional)
        /// - User Secrets (dev only)
        /// - Environment vars
        /// - Command line args
        /// it also configure logging to Console and Debug, 
        /// <see href="https://github.com/serilog/serilog-aspnetcore/issues/3">but I'm using Serilog in place of those</see>,
        /// the configuration of Serilog is at the beginning of the Main method.
        /// </remarks>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseApplicationInsights()
                .UseSerilog()
                .UseStartup<Startup>();
    }
}