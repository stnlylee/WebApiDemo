using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace WebApiDemo.Api
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddEnvironmentVariables();
                    configHost.AddCommandLine(args);
                })
                .ConfigureLogging((context, config) =>
                {
                    config.AddSerilog();
                    config.AddConsole();
                    config.AddDebug();
                })
                .ConfigureAppConfiguration((context, config) =>
                {
                    IHostEnvironment hostEnv = context.HostingEnvironment;
                    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    config
                        .SetBasePath(hostEnv.ContentRootPath + "/Settings/")
                        .AddJsonFile("appsettings.json", optional: false, true)
                        .AddJsonFile($"appsettings.{env}.json", optional: false, true)
                        .AddJsonFile("logger.json", false, true)
                        .AddJsonFile($"logger.{env}.json", optional: true);

                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);
                })
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseUrls("http://*:5000")
                    .UseStartup<Startup>();
                })
                .UseConsoleLifetime();
    }
}
