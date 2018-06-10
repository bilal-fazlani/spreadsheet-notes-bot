using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SpreadsheetTextCapture.MessageProcessors
{
    public class SetSpreadsheet : IMessageProcessor
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly SpreadsheetIdStore _spreadsheetIdStore;
        private readonly TextParser _textParser;
        private readonly ILogger _logger;

        public SetSpreadsheet(ITelegramBotClient telegramBotClient, SpreadsheetIdStore spreadsheetIdStore, 
            TextParser textParser, ILogger logger)
        {
            _telegramBotClient = telegramBotClient;
            _spreadsheetIdStore = spreadsheetIdStore;
            _textParser = textParser;
            _logger = logger;
        }
        
        public async Task ProcessMessageAsync(Update update)
        {
            try
            {
                string args = _textParser.ParseArgs(update);
                if (string.IsNullOrWhiteSpace(args))
                {
                    await ProvideHelpAsync(update);
                }
                else
                {
                    string spreadsheetId = ExtractSpreadSheetId(args);
                    string chatId = update.Message.Chat.Id.ToString();
                    await _spreadsheetIdStore.SetSpreadSheetIdAsync(chatId, spreadsheetId);

                    await _telegramBotClient.SendTextMessageAsync(chatId, $@"Done

Google spreadsheet is now set to {args}");
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error");
                await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, "Something went wrong");
            }   
        }

        private string ExtractSpreadSheetId(string spreadSheetUrl)
        {
            string regex = "/spreadsheets/d/([a-zA-Z0-9-_]+)";
            Match match = Regex.Match(spreadSheetUrl, regex, RegexOptions.Compiled);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            throw new InvalidSpreadsheetUrlException();
        }
        
        private async Task ProvideHelpAsync(Update update)
        {
            try
            {
                string spreadSheetId = await _spreadsheetIdStore.GetSpreadSheetIdAsync(update.Message.Chat.Id.ToString());

                await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id,
                    $@"Current spreadsheet url: {_spreadsheetIdStore.ConvertSpreadSheetIdToUrl(spreadSheetId)}

To change it, use `/spreadsheet <new url>`", ParseMode.Markdown);
            }
            catch (SpreadSheetNotSetException)
            {
                await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id,
                    $@"There is no spreadsheet set yet.

To set it, use `/spreadsheet <new url>`", ParseMode.Markdown);
            }
        }
    }
}