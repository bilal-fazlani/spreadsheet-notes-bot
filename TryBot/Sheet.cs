using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

namespace TryBot
{
    public class Sheet
    {
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "Context capture app";
        
        UserCredential credential;

        private SheetsService service;

        public async Task Initialise()
        {
            Console.WriteLine("Initialising sheet...");
            using (FileStream stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");

                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
                
                Console.WriteLine("Credential file saved to: " + credPath);
                
                // Create Google Sheets API service.
                service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
            }
            Console.WriteLine("Sheet initialised");
        }

//        public async Task Read()
//        {
//            if (service == null) throw new Exception("please initialise Sheet");
//            
//            // Define request parameters.
//            String range = "A1:A2";
//            SpreadsheetsResource.ValuesResource.GetRequest request =
//                service.Spreadsheets.Values.Get(SpreadsheetId, range);
//            
//            // Prints the names and majors of students in a sample spreadsheet:
//            // https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit
//            ValueRange response = request.Execute();
//            IList<IList<Object>> values = response.Values;
//            if (values != null && values.Count > 0)
//            {
//                Console.WriteLine("Name");
//                foreach (IList<object> row in values)
//                {
//                    // Print columns A and E, which correspond to indices 0 and 4.
//                    Console.WriteLine("{0}", row[0]);
//                }
//            }
//            else
//            {
//                Console.WriteLine("No data found.");
//            }
//        }

        public async Task Note(Message message)
        {
            if (service == null) throw new Exception("please initialise Sheet");

            string newRange = await GetRange();

            await UpdatGoogleSheetinBatch(message, newRange);
        }
        
        private async Task UpdatGoogleSheetinBatch(Message message, string newRange)
        {
            SpreadsheetsResource.ValuesResource.AppendRequest request =
                service.Spreadsheets.Values.Append(new ValueRange()
                {
                    Values = new List<IList<object>>()
                    {
                        new List<object>()
                        {
                            message.Name,
                            message.Comments,
                            message.Sender,
                            message.DateTime
                        }
                    }
                } , Config.SpreadSheetId, newRange);
            
            request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            AppendValuesResponse response = await request.ExecuteAsync();
        }
        
        private async Task<string> GetRange()
        {
            String range = "A:A";

            SpreadsheetsResource.ValuesResource.GetRequest getRequest =
                service.Spreadsheets.Values.Get(Config.SpreadSheetId, range);

            ValueRange getResponse = await getRequest.ExecuteAsync();
            IList<IList<Object>> getValues = getResponse.Values;

            int currentCount = getValues?.Count + 1 ?? 1;

            string newRange = "A" + currentCount + ":A";

            return newRange;
        }
    }
}