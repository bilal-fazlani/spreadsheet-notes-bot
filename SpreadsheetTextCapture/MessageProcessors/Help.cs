﻿using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SpreadsheetTextCapture.MessageProcessors
{
    public class Help : IMessageProcessor
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public Help(ITelegramBotClient telegramBotClient, SpreadsheetIdStore spreadsheetIdStore)
        {
            _telegramBotClient = telegramBotClient;
        }

        public async Task ProcessMessageAsync(Update update)
        {
            string chatId = null;
            
            try
            {
                chatId = update.Message.Chat.Id.ToString();

                string helpText = $@"*/help*
Shows available commands

*/note*
Syntax: `/note <content with optional #hastags>`
Adds a note in google spreadsheet

*/spreadsheet*
Syntax: `/spreadsheet <google spreadsheet url>`
Starts using given spreadsheet to store data

*/authorize*
Syntax: `/authorize`
Authorize the bot to access google spreadsheet
";

                await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, helpText,
                    parseMode: ParseMode.Markdown);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                await _telegramBotClient.SendTextMessageAsync(chatId, "Something went wrong");
            }
        }
    }
}
//[google spreadsheet](https://docs.google.com/spreadsheets/d/{spreadSheetId})