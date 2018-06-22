using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Stateless;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace SpreadsheetTextCapture.StateManagement
{
    public class KeyboardManager
    {
        //todo: remove this hardcoding
        const string HARDCODED_CHAT_ID = "552481329"; 
        
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger _logger;
        private StateMachine<KeyboardState, string> _keyboard;
        private StateMachine<KeyboardState, string>.TriggerWithParameters<string> _setUrlTrigger;
        
        public KeyboardManager(ILogger logger, ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
            _logger = logger;

            //////////////////////////////////////////////////////////////////////////////////

            (_keyboard, _setUrlTrigger) = CreateKeyboard();

            ////////////////////////////////////////////////////////////////////////////////////
        }

        private (StateMachine<KeyboardState, string>, StateMachine<KeyboardState, string>.TriggerWithParameters<string>) CreateKeyboard()
        {
            StateMachine<KeyboardState, string> keyboard = new StateMachine<KeyboardState, string>(KeyboardState.Clear);
            var setUrlTrigger = keyboard.SetTriggerParameters<string>(KeyboardTriggers.ENTER_URL);

            keyboard.Configure(KeyboardState.Clear)
                .OnEntryFrom(setUrlTrigger, OnSetSpreadsheetUrl)
                .OnEntryFrom(KeyboardTriggers.CREATE_NEW, OnCreateNewSpreadsheet)
                .OnEntryFrom(KeyboardTriggers.REVOKE_PERMISSIONS, OnRevokePermissions)
                .OnEntryFrom(KeyboardTriggers.AUTHORIZE, OnAuthorize)
                .OnEntryFrom(KeyboardTriggers.OPEN, OnOpen)
                .OnEntryAsync(OnClear)
                .Permit(KeyboardTriggers.SETTINGS, KeyboardState.SettingsOpen);

            keyboard.Configure(KeyboardState.SettingsOpen)
                .OnEntryAsync(OnOpenSettings)
                .Permit(KeyboardTriggers.BACK, KeyboardState.Clear)
                .Permit(KeyboardTriggers.CANCEL, KeyboardState.Clear)
                .Permit(KeyboardTriggers.SPREADSHEET, KeyboardState.SpreadsheetSettingsOpen)
                .Permit(KeyboardTriggers.AUTHORIZATION, KeyboardState.AuthMenuOpen);

            keyboard.Configure(KeyboardState.SpreadsheetSettingsOpen)
                .OnEntryAsync(OnSpreadsheetSettingsOpen)
                .Permit(KeyboardTriggers.BACK, KeyboardState.SettingsOpen)
                .Permit(KeyboardTriggers.CANCEL, KeyboardState.Clear)
                .Permit(KeyboardTriggers.SET_SPREADSHEET, KeyboardState.AwaitingSpreadsheetUrl)
                .Permit(KeyboardTriggers.CHANGE_SPREADSHEET, KeyboardState.AwaitingSpreadsheetUrl)
                .Permit(KeyboardTriggers.CREATE_NEW, KeyboardState.Clear)
                .Permit(KeyboardTriggers.OPEN, KeyboardState.Clear);

            keyboard.Configure(KeyboardState.AwaitingSpreadsheetUrl)
                .OnEntryAsync(OnAwaitingSpreadsheetUrl)
                .Permit(KeyboardTriggers.CANCEL, KeyboardState.Clear)
                .Permit(KeyboardTriggers.ENTER_URL, KeyboardState.Clear);

            keyboard.Configure(KeyboardState.AuthMenuOpen)
                .OnEntryAsync(OnAuthMenuOpen)
                .Permit(KeyboardTriggers.BACK, KeyboardState.SettingsOpen)
                .Permit(KeyboardTriggers.CANCEL, KeyboardState.Clear)
                .Permit(KeyboardTriggers.REVOKE_PERMISSIONS, KeyboardState.Clear)
                .Permit(KeyboardTriggers.AUTHORIZE, KeyboardState.Clear);

            return (keyboard, setUrlTrigger);
        }

        #region status_check

        public bool IsClear()
        {
            return _keyboard.State == KeyboardState.Clear;
        }

        public bool IsAwaitingUrl()
        {
            return _keyboard.State == KeyboardState.AwaitingSpreadsheetUrl;
        }

        public bool CanFire(string input)
        {
            return _keyboard.CanFire(input);
        }
        
        #endregion

        #region control

        public async Task FireAsync(string trigger)
        {
            if(_keyboard.CanFire(trigger))
                await _keyboard.FireAsync(trigger);
        }

        public async Task SetSpreadsheetUrl(string urlInput)
        {
            if(urlInput == KeyboardTriggers.CANCEL)
                await FireAsync(urlInput);
            
            if(_keyboard.CanFire(KeyboardTriggers.ENTER_URL))
                await _keyboard.FireAsync(_setUrlTrigger, urlInput);
        }
        
        public async Task ClearKeyboardAsync()
        {
            _logger.Debug("resetting state");
            (_keyboard, _setUrlTrigger) = CreateKeyboard();
            await ClearKeyboard("Ok");
            _logger.Debug("state is reset");
        }

        
        #endregion

        #region events

        private async Task OnClear()
        {
            _logger.Debug("Keybaord clear initiated");
            await ClearKeyboard(":)");
            _logger.Debug("Keybaord cleared");
        }

        private void OnCreateNewSpreadsheet()
        {
            _logger.Information("A new spreadsheet has been created");
        }

        private async Task OnOpenSettings()
        {
            _logger.Debug("Open settigs initiated");
            await SendKeyboard(
                new[] {KeyboardTriggers.SPREADSHEET, KeyboardTriggers.AUTHORIZATION},
                new[] {KeyboardTriggers.BACK});
            _logger.Debug("Settings menu is now open");
        }

        private async Task OnAuthMenuOpen()
        {
            _logger.Debug("Open Auth settings initialed");
            
            //todo: fix this based on current state of authorisation
            await SendKeyboard(
                new[] {KeyboardTriggers.REVOKE_PERMISSIONS, KeyboardTriggers.AUTHORIZE},
                new[] {KeyboardTriggers.BACK});
            
            _logger.Debug("Auth settings are now open");
        }

        private async Task OnSpreadsheetSettingsOpen()
        {
            _logger.Debug("Open Spreadsheet settings initialed");

            //todo: fix this based on current state of spreadsheet
            await SendKeyboard(
                new [] {KeyboardTriggers.CHANGE_SPREADSHEET, KeyboardTriggers.SET_SPREADSHEET}, 
                new [] {KeyboardTriggers.CREATE_NEW, KeyboardTriggers.BACK});
            
            _logger.Debug("Spreadsheet settings are now open");
        }

        private async Task OnAwaitingSpreadsheetUrl()
        {
            ReplyKeyboardRemove clearKeyboard = new ReplyKeyboardRemove();
            await _telegramBotClient.SendTextMessageAsync(HARDCODED_CHAT_ID, "Ok, please enter the url of spreadsheet.\n" +
                                                                             "To cancel, enter /cancel", replyMarkup: clearKeyboard);
            _logger.Debug("I am now awaiting a new spreadsheet url");
        }

        private void OnSetSpreadsheetUrl(string url)
        {
            _logger.Information($"Spreadsheet url  is now set to {url}");
        }

        private void OnRevokePermissions()
        {
            _logger.Information("Permission revoke url has been sent to user");
        }

        private void OnAuthorize()
        {
            _logger.Information("Authorization URL has been sent to user");
        }
        
        private void OnOpen()
        {
            _logger.Information("Spreadsheet url has been sent to user");
        }
        
        #endregion
        
        private async Task SendKeyboard(params string[][] buttonRows)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(buttonRows.Select(r => r.Select(t=>new KeyboardButton(t))));
            await _telegramBotClient.SendTextMessageAsync(HARDCODED_CHAT_ID, "Select an option", replyMarkup: replyKeyboardMarkup);
        }
        
        private async Task ClearKeyboard(string message)
        {
            ReplyKeyboardRemove clearKeyboard = new ReplyKeyboardRemove();
            await _telegramBotClient.SendTextMessageAsync(HARDCODED_CHAT_ID, message, replyMarkup: clearKeyboard);
        }        
    }
}