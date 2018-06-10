using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

namespace SpreadsheetTextCapture
{
    public class SheetsServiceFactory
    {
        private readonly GoogleAuthentication _googleAuthentication;
        
        public SheetsServiceFactory(GoogleAuthentication googleAuthentication)
        {
            _googleAuthentication = googleAuthentication;
        }

        public async Task<SheetsService> GetSheetsServiceAsync(string chatId)
        {
            TokenResponse tokenResponse = await _googleAuthentication.GetAccessTokenAsync(chatId);

            SheetsService service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = new UserCredential(_googleAuthentication.AuthorizationCodeFlow, chatId, tokenResponse)
            });

            return service;
        }
    }
}