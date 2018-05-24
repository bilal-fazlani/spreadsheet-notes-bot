using Telegram.Bot;

namespace TryBot
{
    public static class BotClientFactory
    {        
        static TelegramBotClient botClient = new TelegramBotClient(Config.TelegramApiKey);
        
        public static ITelegramBotClient GetInstance()
        {
            return botClient;
        }
    }
}