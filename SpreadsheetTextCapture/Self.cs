using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SpreadsheetTextCapture
{
    public class Self
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private User _user;
        
        public Self(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }
        
        public async Task<User> GetInfoAsync()
        {
            if(_user == null)
                _user = await _telegramBotClient.GetMeAsync();
            return _user;
        }
    }
}