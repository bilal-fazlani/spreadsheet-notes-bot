using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Options;

namespace SpreadsheetTextCapture
{
    public class GoogleAuthentication
    {
        private readonly AuthDataStore _authDataStore;
        private readonly AccessCodeStore _accessCodeStore;
        private readonly BotConfig _botConfig;
        
        public GoogleAuthentication(IOptions<BotConfig> options, AuthDataStore authDataStore, AccessCodeStore accessCodeStore)
        {
            _authDataStore = authDataStore;
            _accessCodeStore = accessCodeStore;
            _botConfig = options.Value;
            
            ClientSecrets clientSecrets = new ClientSecrets
            {
                ClientId = _botConfig.GoogleClientId,
                ClientSecret = _botConfig.GoogleClientSecret
            };

            var scopes = new[] {SheetsService.Scope.Spreadsheets};
                //Google.Apis.Oauth2.v2.Oauth2Service.Scope.UserinfoEmail };
            
            GoogleAuthorizationCodeFlow.Initializer initializer = new GoogleAuthorizationCodeFlow.Initializer { 
                ClientSecrets = clientSecrets,
                Scopes = scopes, 
                DataStore = _authDataStore
            };
                
            AuthorizationCodeFlow = new GoogleAuthorizationCodeFlow(initializer);
        }


        public AuthorizationCodeFlow AuthorizationCodeFlow { get; }

        public async Task<TokenResponse> GetAccessTokenAsync(string chatId)
        {
            TokenResponse tokenResponse = await _authDataStore.GetAsync<TokenResponse>(chatId);

            if (tokenResponse == null) // its the first time spreadsheet access for this chat id
            {
                var accessCode = await _accessCodeStore.GetCodeAsync(chatId);
                if (accessCode == null) //user never authorized
                {
                    throw new UnauthorizedChatException(chatId);
                }
                
                else //user authenticatedd but access token was never generated
                {
                    var codeFlow = AuthorizationCodeFlow;

                    //generate access token for the first time
                    tokenResponse = await codeFlow.ExchangeCodeForTokenAsync(
                        chatId,
                        accessCode, 
                        _botConfig.AuthCallbackUrl,
                        CancellationToken.None);
                }
            }

            return tokenResponse;
        }
    }
}