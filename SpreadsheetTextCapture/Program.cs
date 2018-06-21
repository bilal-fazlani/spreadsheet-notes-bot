using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Telegram;

namespace SpreadsheetTextCapture
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        
        public static int Main0(string[] args)
        {
            string telegramApiKey = Configuration.GetSection("BotConfig")["TelegramApiKey"];
            string telegramSinkChatId = Configuration.GetSection("BotConfig")["TelegramSinkChatId"];
            
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Telegram(telegramApiKey, telegramSinkChatId, restrictedToMinimumLevel:LogEventLevel.Error)
                .CreateLogger();
            try
            {
                Log.Information("Getting the motors running...");
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
                .UseConfiguration(Configuration)
                .UseUrls($"http://*:{port}")
                .UseSerilog()
                .ConfigureServices(sc => { sc.AddSingleton<ILogger>(Log.Logger); })
                .UseStartup<Startup>();   
        }
    }
}