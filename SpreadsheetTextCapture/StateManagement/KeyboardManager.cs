using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Options;
using Serilog;
using SpreadsheetTextCapture.DataStores;
using SpreadsheetTextCapture.Exceptions;
using Stateless;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SpreadsheetTextCapture.StateManagement
{
    public class KeyboardManager
    {
        //todo: remove this hardcoding
        const string HARDCODED_CHAT_ID = "552481329";

        private readonly BotConfig _botConfig;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly AccessCodeStore _accessCodeStore;
        private readonly SpreadsheetIdStore _spreadsheetIdStore;
        private readonly SpreadsheetDriver _spreadsheetDriver;
        private readonly ILogger _logger;
        private StateMachine<KeyboardState, string> _keyboard;
        private StateMachine<KeyboardState, string>.TriggerWithParameters<string> _setUrlTrigger;
        
        public KeyboardManager(ILogger logger, ITelegramBotClient telegramBotClient, 
            AccessCodeStore accessCodeStore, SpreadsheetIdStore spreadsheetIdStore,
            IOptions<BotConfig> options, SpreadsheetDriver spreadsheetDriver)
        {
            _telegramBotClient = telegramBotClient;
            _accessCodeStore = accessCodeStore;
            _spreadsheetIdStore = spreadsheetIdStore;
            _spreadsheetDriver = spreadsheetDriver;
            _logger = logger;
            _botConfig = options.Value;

            //////////////////////////////////////////////////////////////////////////////////

            (_keyboard, _setUrlTrigger) = CreateKeyboard();

            ////////////////////////////////////////////////////////////////////////////////////
        }

        private (StateMachine<KeyboardState, string>, StateMachine<KeyboardState, string>.TriggerWithParameters<string>) CreateKeyboard()
        {
            StateMachine<KeyboardState, string> keyboard = new StateMachine<KeyboardState, string>(KeyboardState.Clear);
            
            var setUrlTrigger = keyboard.SetTriggerParameters<string>(KeyboardTriggers.ENTER_URL);

            keyboard.Configure(KeyboardState.Clear)
                .OnEntryFromAsync(setUrlTrigger, OnSetSpreadsheetUrl)
                .OnEntryFromAsync(KeyboardTriggers.CREATE_NEW, OnCreateNewSpreadsheet)
                .OnEntryFromAsync(KeyboardTriggers.REVOKE_PERMISSIONS, OnRevokePermissions)
                .OnEntryFromAsync(KeyboardTriggers.AUTHORIZE, OnAuthorize)
                .OnEntryFromAsync(KeyboardTriggers.AUTHORIZE_AGAIN, OnAuthorize)
                .OnEntryFromAsync(KeyboardTriggers.OPEN, OnOpen)
                .OnEntryFromAsync(KeyboardTriggers.BACK, OnClear)
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
                .Permit(KeyboardTriggers.AUTHORIZE_AGAIN, KeyboardState.Clear)
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
                await FireAsync(KeyboardTriggers.CANCEL);
            
            if(_keyboard.CanFire(KeyboardTriggers.ENTER_URL))
                await _keyboard.FireAsync(_setUrlTrigger, urlInput);
        }
        
        public async Task OnClear()
        {
            _logger.Debug("resetting state");
            (_keyboard, _setUrlTrigger) = CreateKeyboard();
            await ClearKeyboard("Ok");
            _logger.Debug("state is reset");
        }

        
        #endregion

        #region events

        private async Task OnCreateNewSpreadsheet()
        {
            //todo: exception handling
            string url = await _spreadsheetDriver.CreateNewSpreadsheet(HARDCODED_CHAT_ID);
            await ClearKeyboard($@"New spreadsheet has been created here - 
{url}");
        }

        private async Task OnOpenSettings()
        {
            _logger.Debug("Open settings initiated");
            await SendKeyboard(
                new[] {KeyboardTriggers.SPREADSHEET, KeyboardTriggers.AUTHORIZATION},
                new[] {KeyboardTriggers.BACK});
            _logger.Debug("Settings menu is now open");
        }

        private async Task OnAuthMenuOpen()
        {
            _logger.Debug("Open Auth settings initialed");

            bool authorized = await _accessCodeStore.GetCodeAsync(HARDCODED_CHAT_ID) != null;

            await SendKeyboard(
                authorized ? new[] { KeyboardTriggers.REVOKE_PERMISSIONS, KeyboardTriggers.AUTHORIZE_AGAIN } : new[] { KeyboardTriggers.AUTHORIZE },
                new[] {KeyboardTriggers.BACK});
            
            _logger.Debug("Auth settings are now open");
        }

        private async Task OnSpreadsheetSettingsOpen()
        {
            _logger.Debug("Open Spreadsheet settings initialed");

            bool spreadSheetSet = await _spreadsheetIdStore.GetSpreadSheetIdAsync(HARDCODED_CHAT_ID) != null;
            
            await SendKeyboard(
                new []
                {
                    (spreadSheetSet ? KeyboardTriggers.CHANGE_SPREADSHEET : KeyboardTriggers.SET_SPREADSHEET),
                    KeyboardTriggers.CREATE_NEW
                }, 
                spreadSheetSet ? new [] {KeyboardTriggers.OPEN, KeyboardTriggers.BACK} : new [] {KeyboardTriggers.BACK}
                );
            
            _logger.Debug("Spreadsheet settings are now open");
        }

        private async Task OnAwaitingSpreadsheetUrl()
        {
            ReplyKeyboardRemove clearKeyboard = new ReplyKeyboardRemove();
            await _telegramBotClient.SendTextMessageAsync(HARDCODED_CHAT_ID, "Ok, please enter the url of spreadsheet.\n" +
                                                                             "To cancel, enter /cancel", replyMarkup: clearKeyboard);
            _logger.Debug("I am now awaiting a new spreadsheet url");
        }

        private async Task OnSetSpreadsheetUrl(string url)
        {
            (string spreadsheetId, _) = ExtractSpreadSheetId(url);

            await _spreadsheetIdStore.SetSpreadSheetIdAsync(HARDCODED_CHAT_ID, spreadsheetId);
            await ClearKeyboard($@"Done

Google spreadsheet is now set to {url}");
        }

        private async Task OnRevokePermissions()
        {
            await ClearKeyboard("To revoke permissions, visit https://myaccount.google.com/permissions");
        }

        private async Task OnAuthorize()
        {
            string authUrl = "https://accounts.google.com/o/oauth2/v2/auth?" +
                             $"client_id={_botConfig.GoogleClientId}" +
                             $"&redirect_uri={_botConfig.AuthCallbackUrl}" +
                             $"&scope={SheetsService.Scope.Spreadsheets}" +
                             "&response_type=code" +
                             "&access_type=offline" +
                             "&prompt=consent" +
                             $"&state=chatId={HARDCODED_CHAT_ID}";

            string message = $"[Click here]({authUrl}) to authorize me";

            await ClearKeyboard(message, ParseMode.Markdown);
        }
        
        private async Task OnOpen()
        {
            string spreadsheetUrl = await _spreadsheetIdStore.GetSpreadSheetUrlAsync(HARDCODED_CHAT_ID);
            string message = $"[Click here]({spreadsheetUrl}) to open google spreadsheet";
            await ClearKeyboard(message, ParseMode.Markdown);
        }
        
        #endregion
        
        private (string id, bool success) ExtractSpreadSheetId(string spreadSheetUrl)
        {
            string regex = "/spreadsheets/d/([a-zA-Z0-9-_]+)";
            Match match = Regex.Match(spreadSheetUrl, regex, RegexOptions.Compiled);
            if (match.Success)
            {
                return (match.Groups[1].Value, true);
            }

            return (spreadSheetUrl, false);
        }
        
        private async Task SendKeyboard(params string[][] buttonRows)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(buttonRows.Select(r => r.Select(t=>new KeyboardButton(t))));
            await _telegramBotClient.SendTextMessageAsync(HARDCODED_CHAT_ID, "Select an option or " +
                                                                             "send /cancel to cancel current operation", replyMarkup: replyKeyboardMarkup);
        }
        
        private async Task ClearKeyboard(string message, ParseMode parseMode = ParseMode.Default)
        {
            ReplyKeyboardRemove clearKeyboard = new ReplyKeyboardRemove();
            await _telegramBotClient.SendTextMessageAsync(HARDCODED_CHAT_ID, message, replyMarkup: clearKeyboard, parseMode: parseMode);
        }        
    }
}