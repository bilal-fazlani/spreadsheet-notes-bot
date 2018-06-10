using System;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SpreadsheetTextCapture.MessageProcessors
{
    public class Joined : IMessageProcessor
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger _logger;

        private static string JoinText = @"Hi!
Thank you for inviting me to this wonderful group. To know about available commands, try /help";

        public Joined(ITelegramBotClient telegramBotClient, ILogger logger)
        {
            _telegramBotClient = telegramBotClient;
            _logger = logger;
        }
        
        public async Task ProcessMessageAsync(Update update)
        {
            try
            {
                await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, JoinText);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error");
                await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, "Something went wrong");
            }
        }
    }
}