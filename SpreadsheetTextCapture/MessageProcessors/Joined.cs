using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SpreadsheetTextCapture.MessageProcessors
{
    public class Joined : IMessageProcessor
    {
        private readonly ITelegramBotClient _telegramBotClient;

        private static string JoinText = @"Hi!
Thank you for inviting me to this wonderful group. To know about available commands, try /help";

        public Joined(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }
        
        public async Task ProcessMessageAsync(Update update)
        {
            try
            {
                await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, JoinText);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, "Something went wrong");
            }
        }
    }
}