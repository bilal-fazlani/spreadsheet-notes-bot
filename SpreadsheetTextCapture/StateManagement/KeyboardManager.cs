using Serilog;
using Stateless;
using Telegram.Bot;

namespace SpreadsheetTextCapture.StateManagement
{
    public class KeyboardManager
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger _logger;
        private readonly StateMachine<KeyboardState, KeyboardTriggers> _keyboard = new StateMachine<KeyboardState, KeyboardTriggers>(KeyboardState.Clear);
        private readonly StateMachine<KeyboardState, KeyboardTriggers>.TriggerWithParameters<string> _setUrlTrigger;
        
        public KeyboardManager(ITelegramBotClient telegramBotClient, ILogger logger)
        {
            _telegramBotClient = telegramBotClient;
            _logger = logger;

            //////////////////////////////////////////////////////////////////////////////////

            _keyboard.SetTriggerParameters<string>(KeyboardTriggers.UserEnterUrl);
            
            _keyboard.Configure(KeyboardState.Clear)
                .Permit(KeyboardTriggers.UserOpenSettings, KeyboardState.SettingsOpen);

            _keyboard.Configure(KeyboardState.SettingsOpen)
                .OnEntry(OnOpenSettings)
                .Permit(KeyboardTriggers.UserClickBack, KeyboardState.Clear)
                .Permit(KeyboardTriggers.UserClickSpreadsheet, KeyboardState.SpreadsheetSettingsOpen)
                .Permit(KeyboardTriggers.UserClickAuthorization, KeyboardState.AuthMenuOpen);

            _keyboard.Configure(KeyboardState.SpreadsheetSettingsOpen)
                .OnEntry(OnSpreadsheetSettingsOpen)
                .Permit(KeyboardTriggers.UserClickBack, KeyboardState.SettingsOpen)
                .Permit(KeyboardTriggers.UserClickSetUrl, KeyboardState.AwaitingSpreadsheetUrl)
                .Permit(KeyboardTriggers.UserClickCreateNew, KeyboardState.Clear)
                .Permit(KeyboardTriggers.UserClickOpen, KeyboardState.Clear);

            _keyboard.Configure(KeyboardState.AwaitingSpreadsheetUrl)
                .OnEntry(OnAwaitingSpreadsheetUrl)
                .Permit(KeyboardTriggers.UserClickBack, KeyboardState.SettingsOpen)
//                .PermitDynamic(_setUrlTrigger, _ => KeyboardState.SpreadsheetSettingsOpen);
//                .Permit(KeyboardTriggers.UserEnterUrl, KeyboardState.SpreadsheetSettingsOpen);
                .InternalTransition(_setUrlTrigger, (url, transition) => OnSetSpreadsheetUrl(url));


            _keyboard.Configure(KeyboardState.AuthMenuOpen)
                .OnEntry(OnAuthMenuOpen)
                .Permit(KeyboardTriggers.UserClickBack, KeyboardState.SettingsOpen)
                .Permit(KeyboardTriggers.UserClickRevokePermissions, KeyboardState.AuthMenuOpen)
                .Permit(KeyboardTriggers.UserClickAuthorize, KeyboardState.AuthMenuOpen);
            
            ////////////////////////////////////////////////////////////////////////////////////
        }

        public void OpenSettings()
        {
            _keyboard.Fire(KeyboardTriggers.UserOpenSettings);
        }

        private void OnOpenSettings()
        {
            _logger.Information("I am now in open settings state");
        }

        public void GoBack()
        {
            _keyboard.Fire(KeyboardTriggers.UserClickBack);
        }

        public void OpenAuthorizationSettings()
        {
            _keyboard.Fire(KeyboardTriggers.UserClickAuthorization);
        }

        private void OnAuthMenuOpen()
        {
            _logger.Information("Auth menu is now open");
        }

        public void OpenSpreadsheetSettings()
        {
            _keyboard.Fire(KeyboardTriggers.UserClickSpreadsheet);
        }

        private void OnSpreadsheetSettingsOpen()
        {
            _logger.Information("spreadsheet settings are now open");
        }

        public void CreateNewSpreadsheet()
        {
            _keyboard.Fire(KeyboardTriggers.UserClickCreateNew);
        }

        public void PromptForNewSpreadsheetUrl()
        {
            _keyboard.Fire(KeyboardTriggers.UserClickSetUrl);
        }

        private void OnAwaitingSpreadsheetUrl()
        {
            _logger.Information("I am now awaiting a new url");
        }

        public void OpenSpreadsheetUrl()
        {
            _keyboard.Fire(KeyboardTriggers.UserClickOpen);
        }

        public void OpenRevokePermissionsUrl()
        {
            _keyboard.Fire(KeyboardTriggers.UserClickRevokePermissions);
        }

        public void OpenGoogleAuthorizationUrl()
        {
            _keyboard.Fire(KeyboardTriggers.UserClickAuthorize);
        }

        public void SetSpreadsheetUrl(string url)
        {
            _logger.Information($"Spreadsheet url has been set to {url}");
            _keyboard.Fire(KeyboardTriggers.UserEnterUrl);
        }

        private void OnSetSpreadsheetUrl(string url)
        {
            _logger.Information($"spreadsheet url  is now set to {url}");
        }
    }
}