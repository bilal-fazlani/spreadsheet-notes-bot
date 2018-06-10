using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SpreadsheetTextCapture.DataStores;
using Telegram.Bot;

namespace SpreadsheetTextCapture.Controllers
{
    [ApiController]
    public class AuthCallbackController : ControllerBase
    {
        private readonly AccessCodeStore _accessCodeStore;
        private readonly TextParser _textParser;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger _logger;
        private readonly GoogleAuthentication _googleAuthentication;

        public AuthCallbackController(AccessCodeStore accessCodeStore, TextParser textParser, 
            ITelegramBotClient telegramBotClient, ILogger logger, GoogleAuthentication googleAuthentication)
        {
            _accessCodeStore = accessCodeStore;
            _textParser = textParser;
            _telegramBotClient = telegramBotClient;
            _logger = logger;
            _googleAuthentication = googleAuthentication;
        }
        
        [HttpGet]
        [Route("/callback")]
        public async Task<IActionResult> GoogleCallbackForCode([FromQuery(Name = "code")]string accessCode, string state)
        {            
            string chatId = _textParser.ParseCallbackState(state)["chatId"];
            
            _logger.Debug("Received auth callback for chatId - {chatId}", chatId);
            
            await _accessCodeStore.SetAccessCodeAsync(chatId, accessCode);
            
            _logger.Debug("Access code for chat id - {chatId} saved", chatId);

            await _googleAuthentication.GetAccessTokenAsync(chatId);
            
            //todo:test spreadsheet accesss
            
            await _telegramBotClient.SendTextMessageAsync(chatId, @"Successfully authenticated with google.

To revoke permissions, visit https://myaccount.google.com/permissions");
  
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