using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace TryBot.Commands
{
    public class Help : IMessageProcessor
    {
        static ITelegramBotClient botClient = BotClientFactory.GetInstance();

        private static string HelpText = $@"*/help*
Shows available commands

*/note*
Syntax: `/note <Name> <Context>`
Adds context about a person in [google spreadsheet](https://docs.google.com/spreadsheets/d/{Config.SpreadSheetId})
";
        
        public async Task ProcessMessage(MessageEventArgs messageEventArgs)
        {
            try
            {
                await botClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id, HelpText, parseMode: ParseMode.Markdown);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                await botClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id, "Something went wrong");
            }
        }
    }
}