namespace SpreadsheetTextCapture
{
    public class BotConfig
    {
        public string TelegramApiKey { get; set; }
        public string TelegramWebhookUrl { get; set; }
        public string TelegramSinkChatId { get; set; }
        public string MongoConnectionString { get; set; }
        public string MongoDatabaseName { get; set; }
        public string GoogleClientId { get; set; }
        public string GoogleClientSecret { get; set; }
        public string AuthCallbackUrl { get; set; }
        
    }
}