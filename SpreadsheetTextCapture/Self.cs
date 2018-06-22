using System.Threading.Tasks;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SpreadsheetTextCapture
{
    public class Self
    {        
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger _logger;
        private User _user;
        
        public Self(ITelegramBotClient telegramBotClient, ILogger logger)
        {
            _telegramBotClient = telegramBotClient;
            _logger = logger;
        }
        
        public async Task<User> GetInfoAsync()
        {
            if (_user == null)
            {
                _logger.Debug("fetching information about self...");
                _user = await _telegramBotClient.GetMeAsync();
                _logger.Information("fetched info about self");
            }
            
            return _user;
        }
    }
}