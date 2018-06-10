using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace SpreadsheetTextCapture
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            string port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
            
            return WebHost.CreateDefaultBuilder(args)
                .UseUrls($"http://*:{port}")
                .UseStartup<Startup>();   
        }
    }
}