using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace TryBot.Commands
{
    public class Joined : IMessageProcessor
    {
        static ITelegramBotClient botClient = BotClientFactory.GetInstance();
        
        private static string JoinText = @"Hi!
Thank you for inviting me to this wonderful group. To know about available commands, try /help";
        
        public async Task ProcessMessage(MessageEventArgs messageEventArgs)
        {
            try
            {
                await botClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id, JoinText);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                await botClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id, "Something went wrong");
            }
        }
    }
}