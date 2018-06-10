using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Serilog;
using SpreadsheetTextCapture.DataStores;
using Telegram.Bot.Types;

namespace SpreadsheetTextCapture
{
    public class SpreadsheetDriver
    {
        private readonly SheetsServiceFactory _sheetsServiceFactory;
        private readonly SpreadsheetIdStore _spreadsheetIdStore;
        private readonly ILogger _logger;

        public SpreadsheetDriver(SheetsServiceFactory sheetsServiceFactory, 
            SpreadsheetIdStore spreadsheetIdStore,
            ILogger logger)
        {
            _sheetsServiceFactory = sheetsServiceFactory;
            _spreadsheetIdStore = spreadsheetIdStore;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new google spreadsheet
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns>Url of newly created spreadsheet</returns>
        public async Task<string> CreateNewSpreadsheet(ChatId chatId)
        {
            _logger.Debug("creating new google spreadsheet for chat {chatId}", chatId);
            
            SheetsService sheetsService = await _sheetsServiceFactory.GetSheetsServiceAsync(chatId);
            
            DateTime now = DateTime.Now;
            
            string sheetName = $"bot-spreadsheet-{now.Day}-{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(now.Month)}-{now.Year}";

            Spreadsheet newSheet = new Spreadsheet {Properties = new SpreadsheetProperties {Title = sheetName}};
            
            Spreadsheet newSheetResponse = await sheetsService.Spreadsheets.Create(newSheet).ExecuteAsync();
            
            _logger.Information("new spreadsheet {spreadsheet id} created for chat {chatId}", 
                newSheetResponse.SpreadsheetId, chatId);

            await _spreadsheetIdStore.SetSpreadSheetIdAsync(chatId, newSheetResponse.SpreadsheetId);
            
            await AppendMessageAsync(sheetsService, newSheetResponse.SpreadsheetId, new Message("Note", "Added on", "Added by"));
            
            return _spreadsheetIdStore.ConvertSpreadSheetIdToUrl(newSheetResponse.SpreadsheetId);
        }
        
        public async Task Note(string chatId, Message message)
        {
            SheetsService sheetsService = await _sheetsServiceFactory.GetSheetsServiceAsync(chatId);

            string spreadSheetId = await _spreadsheetIdStore.GetSpreadSheetIdAsync(chatId);

            await AppendMessageAsync(sheetsService, spreadSheetId, message);
        }
        
        private async Task AppendMessageAsync(SheetsService service, string spreadsheetId, Message message)
        {
            string range = await GetRange(service, spreadsheetId);
            
            SpreadsheetsResource.ValuesResource.AppendRequest request =
                service.Spreadsheets.Values.Append(new ValueRange
                {
                    Values = new List<IList<object>>()
                    {
                        new List<object>
                        {
                            message.AddedBy,
                            message.DateTime,
                            message.Comment
                        }.Union(message.Tags).ToList()
                    }
                } , spreadsheetId, range);
            
            request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            AppendValuesResponse response = await request.ExecuteAsync();
        }
        
        private async Task<string> GetRange(SheetsService service, string spreadsheetId)
        {
            string range = "A:A";

            SpreadsheetsResource.ValuesResource.GetRequest getRequest =
                service.Spreadsheets.Values.Get(spreadsheetId, range);

            ValueRange getResponse = await getRequest.ExecuteAsync();
            IList<IList<Object>> getValues = getResponse.Values;

            int currentCount = getValues?.Count + 1 ?? 1;

            string newRange = "A" + currentCount + ":A";

            return newRange;
        }
    }
}