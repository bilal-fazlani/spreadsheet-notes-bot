using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace TryBot.Commands
{
    public class Start : IMessageProcessor
    {
        static ITelegramBotClient botClient = BotClientFactory.GetInstance();
        
        private static string StartText = $@"Hi!
I am your assistant. My name is {Self.GetInfoAsync().Result.FirstName}. To know about available commands, try /help";
        
        public async Task ProcessMessage(MessageEventArgs messageEventArgs)
        {
            try
            {
                await botClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id, StartText);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                await botClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id, "Something went wrong");
            }
        }
    }
}