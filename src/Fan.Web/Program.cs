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
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
               .ReadFrom.Configuration(configuration)
               .Enrich.FromLogContext()
               .CreateLogger();

            try
            {
                Log.Information("Starting web host");

                BuildWebHost(args).Run();
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
        /// Build WebHost.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
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
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseSerilog()
                .UseStartup<Startup>()
                .Build();
    }
}