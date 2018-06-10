using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;

namespace SpreadsheetTextCapture.Controllers
{
    [ApiController]
    public class AuthCallbackController : ControllerBase
    {
        private readonly AccessCodeStore _accessCodeStore;
        private readonly TextParser _textParser;
        private readonly ITelegramBotClient _telegramBotClient;

        public AuthCallbackController(AccessCodeStore accessCodeStore, TextParser textParser, ITelegramBotClient telegramBotClient)
        {
            _accessCodeStore = accessCodeStore;
            _textParser = textParser;
            _telegramBotClient = telegramBotClient;
        }
        
        [HttpGet]
        [Route("/callback")]
        public async Task<IActionResult> GoogleCallbackForCode([FromQuery(Name = "code")]string accessCode, string state)
        {            
//            Console.WriteLine($"code: {code}, state: {state}");

//            var codeFlow = _cred.AuthorizationCodeFlow;
//
//            TokenResponse accessToken = await codeFlow.ExchangeCodeForTokenAsync("bilalmf@thoughtworks.com", accessCode, _botConfig.AuthCallbackUrl,
//                CancellationToken.None);

            string chatId = _textParser.ParseCallbackState(state)["chatId"];
            
            await _accessCodeStore.SetAccessCodeAsync(chatId, accessCode);
            
            //todo:test spreadsheet accesss
            
            await _telegramBotClient.SendTextMessageAsync(chatId, @"Successfully authenticated with google.

To revoke permissions, visit https://myaccount.google.com/permissions");
            
            //todo: use serilog
            
//            string token = JsonConvert.SerializeObject(accessToken);
//            
//            Console.WriteLine($"tokenResponse : {token}");
//            
//            SheetsService service = new SheetsService(new BaseClientService.Initializer
//            {
//                HttpClientInitializer = new UserCredential(codeFlow, "bilalmf@thoughtworks.com", accessToken)
//            });
            
//            SpreadsheetsResource.GetRequest request = new SpreadsheetsResource.GetRequest(service, "1aEzYnsxGiJtPiJK2loKteRtT5XvrqkPaE16oNiHbMuw");
                                                                          //
                                                                          //            var spreadsheet = await request.ExecuteAsync();
                                                                          //            
                                                                          //            Console.WriteLine(JsonConvert.SerializeObject(spreadsheet));

            return Redirect("/success");
        }
        
        [HttpGet]
        [Route("/success")]
        public IActionResult Redirect()
        {
            return Ok("Authorisation success. You may close this browser window/tab");
        }
    }
}