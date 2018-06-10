using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog;
using SpreadsheetTextCapture.DataStores;
using SpreadsheetTextCapture.Exceptions;
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
        private readonly SpreadsheetDriver _spreadsheetDriver;

        public SetSpreadsheet(ITelegramBotClient telegramBotClient, 
            SpreadsheetIdStore spreadsheetIdStore, 
            TextParser textParser, 
            ILogger logger,
            SpreadsheetDriver spreadsheetDriver)
        {
            _telegramBotClient = telegramBotClient;
            _spreadsheetIdStore = spreadsheetIdStore;
            _textParser = textParser;
            _logger = logger;
            _spreadsheetDriver = spreadsheetDriver;
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
                else if (args.ToLower() == "new")
                {
                    string url = await _spreadsheetDriver.CreateNewSpreadsheet(update.Message.Chat.Id);
                    await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id,
                        $@"New spreadsheet has been created here - 
{url}");
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

                string response = $@"Current spreadsheet url: `{_spreadsheetIdStore.ConvertSpreadSheetIdToUrl(spreadSheetId)}`

To change it, use `/spreadsheet <spreadsheet url>`

To create a new spreadsheet, use `/spreadsheet new`";
                await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id,
                    response, ParseMode.Markdown);
            }
            catch (SpreadSheetNotSetException)
            {
                var response = @"There is no spreadsheet set yet.

To set it, use `/spreadsheet <spreadsheet url>`

To create a new spreadsheet, use `/spreadsheet new`";
                await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id,
                    response, ParseMode.Markdown);
            }
        }
    }
}