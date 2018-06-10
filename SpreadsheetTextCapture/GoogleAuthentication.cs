using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Sheets.v4;
using Google.Apis.Util;
using Microsoft.Extensions.Options;
using Serilog;

namespace SpreadsheetTextCapture
{
    public class GoogleAuthentication
    {
        private readonly AccessTokenStore _accessTokenStore;
        private readonly AccessCodeStore _accessCodeStore;
        private readonly ILogger _logger;
        private readonly BotConfig _botConfig;
        
        public GoogleAuthentication(IOptions<BotConfig> options, AccessTokenStore accessTokenStore, 
            AccessCodeStore accessCodeStore, ILogger logger)
        {
            _accessTokenStore = accessTokenStore;
            _accessCodeStore = accessCodeStore;
            _logger = logger;
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
                DataStore = _accessTokenStore
            };
                
            AuthorizationCodeFlow = new GoogleAuthorizationCodeFlow(initializer);
        }


        public AuthorizationCodeFlow AuthorizationCodeFlow { get; }

        public async Task<TokenResponse> GetAccessTokenAsync(string chatId)
        {
            TokenResponse tokenResponse = await _accessTokenStore.GetAsync<TokenResponse>(chatId);         
            
            if (tokenResponse == null) // its the first time spreadsheet access for this chat id
            {
                string accessCode = await _accessCodeStore.GetCodeAsync(chatId);
                if (accessCode == null) //user never authorized
                {
                    throw new UnauthorizedChatException(chatId);
                } 
                else //user authenticatedd but access token was never generated
                {
                    //generate access token for the first time
                    _logger.Debug("generating access token for chat id - {chatId}", chatId);
                    
                    tokenResponse = await AuthorizationCodeFlow.ExchangeCodeForTokenAsync(
                        chatId,
                        accessCode,
                        _botConfig.AuthCallbackUrl,
                        CancellationToken.None);
                    
                    _logger.Information("access token generated successfully for chat id - {chatId}", chatId);
                    
                    if(string.IsNullOrEmpty(tokenResponse.RefreshToken))
                    {
                        _logger.Warning("the newly generated access token does not have refresh token in it");
                    }
                }
            }
            else if (tokenResponse.IsExpired(AuthorizationCodeFlow.Clock))
            {
                _logger.Debug("token was expired... refreshing it...");
                
                tokenResponse = await AuthorizationCodeFlow.RefreshTokenAsync(chatId, tokenResponse.RefreshToken,
                    CancellationToken.None);
                
                _logger.Information("token refreshed successfully");
                
                if(string.IsNullOrEmpty(tokenResponse.RefreshToken))
                {
                    _logger.Warning("the newly refreshed access token does not have refresh token in it");
                }
            }   

            return tokenResponse;
        }
    }
}