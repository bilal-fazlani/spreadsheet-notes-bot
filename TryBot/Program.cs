using System;
using System.Threading.Tasks;
using CommandDotNet;
using CommandDotNet.Attributes;
using CommandDotNet.Exceptions;
using CommandDotNet.Models;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using TryBot.Commands;

namespace TryBot
{
    class Program
    {
        static void Main(string[] args)
        {
            AppRunner<App> appRunner = new AppRunner<App>(new AppSettings
            {
                Case = Case.KebabCase
            });

            appRunner.Run(args);
        }
    }

    public class App
    {
        public void Start(string spreadsheetId, string telegramApiKey)
        {
            if (string.IsNullOrEmpty(spreadsheetId) || string.IsNullOrEmpty(telegramApiKey))
                throw new Exception("please provide all parameters");
            
            Config.SpreadSheetId = spreadsheetId;
            Config.TelegramApiKey = telegramApiKey;
            
            ITelegramBotClient botClient = BotClientFactory.GetInstance();

            botClient.OnMessage += BotClientOnMessage;
            botClient.StartReceiving(Array.Empty<UpdateType>());
            
            Console.ReadLine();
            botClient.StopReceiving();
        }
        
        private static async void BotClientOnMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine($"message text: {e?.Message?.Text}");

            IMessageProcessor messageProcessor = await MessageProcessorFactory.GetMessageProcessorAsync(e);
            if (messageProcessor != null) await messageProcessor.ProcessMessage(e);
        }
    }
}