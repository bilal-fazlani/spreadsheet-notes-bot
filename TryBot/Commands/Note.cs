using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace TryBot.Commands
{
    public class Note : IMessageProcessor
    {
        static readonly ITelegramBotClient BotClient = BotClientFactory.GetInstance();
        readonly Sheet _sheet = new Sheet();
        private bool _initialised;
        
        
        public async Task ProcessMessage(MessageEventArgs messageEventArgs)
        {
            try
            {
                if (!_initialised)
                {
                    await _sheet.Initialise();
                    _initialised = true;
                }
                
                string text = TextParser.ParseArgs(messageEventArgs)?.Trim();

                if (string.IsNullOrWhiteSpace(text))
                {
                    await ProvideHelpAsync(messageEventArgs);
                }
                else
                {                    
                    await _sheet.Note(new Message(text, messageEventArgs.Message.Date.ToString("dd-MMM-yyyy"), messageEventArgs.Message.From.FirstName));
                    await BotClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id, "Noted");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                await BotClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id, "Something went wrong");
            }
        }

        private async Task ProvideHelpAsync(MessageEventArgs messageEventArgs)
        {
            await BotClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id, @"Try specifying more parameters.

Syntax: `/note <Name> <Context>`
For example: `/note john wants to work on big data`", ParseMode.Markdown);
        }
    }
}