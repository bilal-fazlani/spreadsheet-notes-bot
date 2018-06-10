using System;
using System.Threading.Tasks;
using Google;
using Google.Apis.Auth.OAuth2.Responses;
using Serilog;
using SpreadsheetTextCapture.DataStores;
using SpreadsheetTextCapture.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SpreadsheetTextCapture.MessageProcessors
{
    public class Note : IMessageProcessor
    {
        private readonly SpreadsheetDriver _spreadsheetDriver;
        private readonly TextParser _textParser;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly Authorize _authorize;
        private readonly AccessTokenStore _accessTokenStore;
        private readonly AccessCodeStore _accessCodeStore;
        private readonly ILogger _logger;

        public Note(SpreadsheetDriver spreadsheetDriver, TextParser textParser, ITelegramBotClient telegramBotClient, 
            Authorize authorize, AccessTokenStore accessTokenStore, AccessCodeStore accessCodeStore, ILogger logger)
        {
            _spreadsheetDriver = spreadsheetDriver;
            _textParser = textParser;
            _telegramBotClient = telegramBotClient;
            _authorize = authorize;
            _accessTokenStore = accessTokenStore;
            _accessCodeStore = accessCodeStore;
            _logger = logger;
        }
        
        public async Task ProcessMessageAsync(Update update)
        {            
            string chatId = null;

            try
            {
                chatId = update.Message.Chat.Id.ToString();

                string text = _textParser.ParseArgs(update)?.Trim();

                if (string.IsNullOrWhiteSpace(text))
                {
                    await ProvideHelpAsync(update);
                }
                else
                {
                    string fromName = $"{update.Message.From.FirstName} {update.Message.From.LastName}".Trim();
                    
                    await _spreadsheetDriver.Note(chatId,
                        new Message(text, update.Message.Date.ToString("dd-MMM-yyyy"), fromName));
                    
                    
                    _logger.Debug("sending success message back to chat {chatId}", chatId);
                    
                    await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, "Noted");
                    
                    _logger.Debug("success message successfully sent back to chat {chat}", chatId);
                }
            }

            catch (TokenResponseException ex)
            {
                _logger.Error(ex, "Error in chat {chatId}", chatId);
                
                //permissions may have been revoked, clear access codes and access tokens
                await _accessCodeStore.DeleteCodeAsync(chatId);
                await _accessTokenStore.DeleteAsync<TokenResponse>(chatId);
                //cleaned codes
                
                await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, "I am having difficulties accessing the spreadsheet. Try authorizing again and make sure you have permissions to make changes to the spreadsheet");
                await _authorize.ProcessMessageAsync(update);
            }
            catch (GoogleApiException ex) when (ex.Error.Code == 401)
            {
                _logger.Error(ex, "Error in chat {chatId}", chatId);
                
                //permissions may have been revoked, clear access codes and access tokens
                await _accessCodeStore.DeleteCodeAsync(chatId);
                await _accessTokenStore.DeleteAsync<TokenResponse>(chatId);
                //cleaned codes
                
                await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, "I am having difficulties accessing the spreadsheet. Try authorizing again and make sure you have permissions to make changes to the spreadsheet");
                await _authorize.ProcessMessageAsync(update);
            }
            catch (SpreadSheetNotSetException)
            {
                await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, "You have not yet set the spreadsheet. Check /spreadsheet for more info");
            }
            catch (UnauthorizedChatException)
            {
                await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, "I am not yet authorized to access your spreadsheets");
                await _authorize.ProcessMessageAsync(update);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in chat {chatId}", chatId);
                await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, "Something went wrong");
            }
        }

        private async Task ProvideHelpAsync(Update update)
        {
            await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, @"Try specifying more parameters.

Syntax: `/note <content with optional #hastags>`
For example: `/note #john wants to work on big data`", ParseMode.Markdown);
        }
    }
}