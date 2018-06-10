using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SpreadsheetTextCapture.MessageProcessors
{
    public class Start : IMessageProcessor
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly Self _self;

        public Start(ITelegramBotClient telegramBotClient, Self self)
        {
            _telegramBotClient = telegramBotClient;
            _self = self;
        }
        
        public async Task ProcessMessageAsync(Update update)
        {
            try
            {
                string firstName = (await _self.GetInfoAsync()).FirstName;
                
                string startText = $@"Hi!
I am your assistant. My name is {firstName}. To know about available commands, try /help";
                
                await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, startText);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, "Something went wrong");
            }
        }
    }
}