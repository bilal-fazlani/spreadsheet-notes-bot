using System;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SpreadsheetTextCapture.MessageProcessors
{
    public class Authorize : IMessageProcessor
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger _logger;
        private readonly BotConfig _botConfig;
        
        public Authorize(IOptions<BotConfig> options, ITelegramBotClient telegramBotClient, ILogger logger)
        {
            _telegramBotClient = telegramBotClient;
            _logger = logger;
            _botConfig = options.Value;
        }
        
        public async Task ProcessMessageAsync(Update update)
        {
            try
            {
                string chatId = update.Message.Chat.Id.ToString();
            
                string authUrl = "https://accounts.google.com/o/oauth2/v2/auth?" +
                                 $"client_id={_botConfig.GoogleClientId}" +
                                 $"&redirect_uri={_botConfig.AuthCallbackUrl}" +
                                 $"&scope={SheetsService.Scope.Spreadsheets}" +
                                 "&response_type=code" +
                                 "&access_type=offline" +
                                 "&prompt=consent" +
                                 $"&state=chatId={chatId}";

                await _telegramBotClient.SendTextMessageAsync(chatId,
                    $"[Click here]({authUrl}) to authorize me", ParseMode.Markdown);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error");
                await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, "Something went wrong");
            }
        }
    }
}