using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace SpreadsheetTextCapture
{
    public class NoteTaker
    {
        private readonly SheetsServiceFactory _sheetsServiceFactory;
        private readonly SpreadsheetIdStore _spreadsheetIdStore;

        public NoteTaker(SheetsServiceFactory sheetsServiceFactory, SpreadsheetIdStore spreadsheetIdStore)
        {
            _sheetsServiceFactory = sheetsServiceFactory;
            _spreadsheetIdStore = spreadsheetIdStore;
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
                service.Spreadsheets.Values.Append(new ValueRange()
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