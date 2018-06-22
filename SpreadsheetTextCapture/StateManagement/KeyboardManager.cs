using System;
using Serilog;
using Stateless;
using Stateless.Graph;
using Telegram.Bot;

namespace SpreadsheetTextCapture.StateManagement
{
    public class KeyboardManager
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger _logger;
        private readonly StateMachine<KeyboardState, string> _keyboard = new StateMachine<KeyboardState, string>(KeyboardState.Clear);
        private readonly StateMachine<KeyboardState, string>.TriggerWithParameters<string> _setUrlTrigger;
        
        public KeyboardManager(ILogger logger)
        {
            //_telegramBotClient = telegramBotClient;
            _logger = logger;

            //////////////////////////////////////////////////////////////////////////////////

            _setUrlTrigger = _keyboard.SetTriggerParameters<string>(KeyboardTriggers.ENTER_URL);
            
            _keyboard.Configure(KeyboardState.Clear)
                .OnEntry(OnClear)
                .OnEntryFrom(_setUrlTrigger, OnSetSpreadsheetUrl)
                .OnEntryFrom(KeyboardTriggers.CREATE_NEW, OnCreateNewSpreadsheet)
                .OnEntryFrom(KeyboardTriggers.REVOKE_PERMISSIONS, OnRevokePermissions)
                .OnEntryFrom(KeyboardTriggers.AUTHORIZE, OnAuthorize)
                .OnEntryFrom(KeyboardTriggers.OPEN, OnOpen)
                .Permit(KeyboardTriggers.SETTINGS, KeyboardState.SettingsOpen);

            _keyboard.Configure(KeyboardState.SettingsOpen)
                .OnEntry(OnOpenSettings)
                .Permit(KeyboardTriggers.BACK, KeyboardState.Clear)
                .Permit(KeyboardTriggers.SPREADSHEET, KeyboardState.SpreadsheetSettingsOpen)
                .Permit(KeyboardTriggers.AUTHORIZATION, KeyboardState.AuthMenuOpen);

            _keyboard.Configure(KeyboardState.SpreadsheetSettingsOpen)
                .OnEntry(OnSpreadsheetSettingsOpen)
                .OnEntryFrom(KeyboardTriggers.CANCEL, OnCancel)
                .Permit(KeyboardTriggers.BACK, KeyboardState.SettingsOpen)
                .Permit(KeyboardTriggers.SET_SPREADSHEET, KeyboardState.AwaitingSpreadsheetUrl)
                .Permit(KeyboardTriggers.CHANGE_SPREADSHEET, KeyboardState.AwaitingSpreadsheetUrl)
                .Permit(KeyboardTriggers.CREATE_NEW, KeyboardState.Clear)
                .Permit(KeyboardTriggers.OPEN, KeyboardState.Clear);

            _keyboard.Configure(KeyboardState.AwaitingSpreadsheetUrl)
                .OnEntry(OnAwaitingSpreadsheetUrl)
                .Permit(KeyboardTriggers.CANCEL, KeyboardState.SpreadsheetSettingsOpen)
                .Permit(KeyboardTriggers.ENTER_URL, KeyboardState.Clear);
                
            _keyboard.Configure(KeyboardState.AuthMenuOpen)
                .OnEntry(OnAuthMenuOpen)
                .Permit(KeyboardTriggers.BACK, KeyboardState.SettingsOpen)
                .Permit(KeyboardTriggers.REVOKE_PERMISSIONS, KeyboardState.Clear)
                .Permit(KeyboardTriggers.AUTHORIZE, KeyboardState.Clear);
            
            ////////////////////////////////////////////////////////////////////////////////////
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

        #endregion

        #region control

        public void Fire(string trigger)
        {
            if(_keyboard.CanFire(trigger))
                _keyboard.Fire(trigger);
        }

        public void SetSpreadsheetUrl(string urlInput)
        {
            if(urlInput == KeyboardTriggers.CANCEL)
                Fire(urlInput);
            
            if(_keyboard.CanFire(KeyboardTriggers.ENTER_URL))
                _keyboard.Fire(_setUrlTrigger, urlInput);
        }

        #endregion

        #region events

        private void OnClear()
        {
            _logger.Information("All clear !");
        }

        private void OnCreateNewSpreadsheet()
        {
            _logger.Information("A new spreadsheet has been created");
        }

        private void OnOpenSettings()
        {
            _logger.Information("Settings are now open");
        }

        private void OnAuthMenuOpen()
        {
            _logger.Information("Auth settings are now open");
        }

        private void OnSpreadsheetSettingsOpen()
        {
            _logger.Information("Spreadsheet settings are now open");
        }

        private void OnAwaitingSpreadsheetUrl()
        {
            _logger.Information("I am now awaiting a new spreadsheet url");
        }

        private void OnSetSpreadsheetUrl(string url)
        {
            _logger.Information($"Spreadsheet url  is now set to {url}");
        }

        private void OnCancel()
        {
            _logger.Information("Cancelled spreadsheet url change");
        }

        private void OnRevokePermissions()
        {
            _logger.Information($"Permission revoke url has been sent to user");
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

        #region temp
        
        public void PrintAvailableCommands()
        {
            Console.WriteLine("Available commands: ");
            foreach (var keyboardPermittedTrigger in _keyboard.PermittedTriggers)
            {
                Console.WriteLine(keyboardPermittedTrigger);
            }
        }

        #endregion
    }
}