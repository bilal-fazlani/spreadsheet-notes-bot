using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace SpreadsheetTextCapture
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
            try
            {
                CreateWebHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            string port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
            
            return WebHost.CreateDefaultBuilder(args)
                .UseUrls($"http://*:{port}")
                .UseSerilog()
                .ConfigureServices(sc => { sc.AddSingleton<ILogger>(Log.Logger); })
                .UseStartup<Startup>();   
        }
    }
}