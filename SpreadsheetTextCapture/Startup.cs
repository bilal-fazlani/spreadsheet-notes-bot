﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SpreadsheetTextCapture.MessageProcessors;
using Telegram.Bot;

namespace SpreadsheetTextCapture
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);
            services.Configure<BotConfig>(Configuration.GetSection("BotConfig"));

            services.AddSingleton<ITelegramBotClient>(s =>
            {
                IOptions<BotConfig> options = s.GetService<IOptions<BotConfig>>();
                BotConfig botConfig = options.Value;
                var botClient = new TelegramBotClient(botConfig.TelegramApiKey);
                botClient.SetWebhookAsync(botConfig.TelegramWebhookUrl).Wait();
                return botClient;
            });

            services.AddSingleton<MessageProcessorFactory>();
            services.AddSingleton<Note>();
            services.AddSingleton<SetSpreadsheet>();
            services.AddSingleton<Joined>();
            services.AddSingleton<Help>();
            services.AddSingleton<Self>();
            services.AddSingleton<Start>();
            services.AddSingleton<Authorize>();
            services.AddSingleton<SpreadsheetIdStore>();
            services.AddSingleton<AuthDataStore>();
            services.AddSingleton<NoteTaker>();
            services.AddSingleton<SheetsServiceFactory>();
            services.AddSingleton<GoogleAuthentication>();
            services.AddSingleton<AccessCodeStore>();
            services.AddSingleton<TextParser>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<SerilogMiddleware>();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
//            else
//            {
//                app.UseHsts();
//            }

            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}