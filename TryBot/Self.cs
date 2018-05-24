using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TryBot
{
    public static class Self
    {
        static ITelegramBotClient botClient = BotClientFactory.GetInstance();

        private static User user;
        
        public static async Task<User> GetInfoAsync()
        {
            if(user == null)
                user = await botClient.GetMeAsync();
            return user;
        }
    }
}