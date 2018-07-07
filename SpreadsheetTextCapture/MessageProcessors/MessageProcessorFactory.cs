using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog;
using SpreadsheetTextCapture.StateManagement;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SpreadsheetTextCapture.MessageProcessors
{
    public class MessageProcessorFactory
    {
        private readonly Note _note;
        private readonly SetSpreadsheet _setSpreadsheet;
        private readonly Authorize _authorize;
        private readonly Start _start;
        private readonly Help _help;
        private readonly Joined _joined;
        private readonly Keyboard _keyboard;
        private readonly KeyboardManager _keyboardManager;
        private readonly Self _self;
        private readonly ILogger _logger;

        public MessageProcessorFactory(Note note, SetSpreadsheet setSpreadsheet, Authorize authorize,
            Start start, Help help, Joined joined, Keyboard keyboard, KeyboardManager keyboardManager,
            Self self, ILogger logger)
        {
            _note = note;
            _setSpreadsheet = setSpreadsheet;
            _authorize = authorize;
            _start = start;
            _help = help;
            _joined = joined;
            _keyboard = keyboard;
            _keyboardManager = keyboardManager;
            _self = self;
            _logger = logger;
        }

        public async Task<IMessageProcessor> GetMessageProcessorAsync(Update update)
        {            
            if (update.Type != UpdateType.Message)
            {
                _logger.Information("unsupported update type. will be ignored {@update}", update);
                return null;
            }
            
            if (update.Message.Type == MessageType.Text)
            {
                string message = update.Message.Text.Trim();
                
                if (!_keyboardManager.IsClear())
                {
                    return _keyboard;
                }
            
                Regex regex = new Regex(@"\/(note|help|start|authorize|spreadsheet|settings|cancel)(.*)", RegexOptions.IgnoreCase);

                Match match = regex.Match(message);

                if (match.Success)
                {
                    string commandName = match.Groups[1].Value;
                    
                    _logger.Debug("command name is: {command}", commandName);
                
                    switch (commandName.ToLower())
                    {
                        case "cancel":
                        case "settings":
                            return _keyboard;
                        case "note":
                            return _note;
                        case "spreadsheet":
                            return _setSpreadsheet;
                        case "authorize":
                            return _authorize;
                        case "start":
                            return _start;
                        case "help":
                            return _help;
                        default:
                            _logger.Error("Could not find a message processor for update. {@update}", update);
                            return null;
                    }
                }
                return _help;
            }

            if (update.Message.Type == MessageType.ChatMembersAdded)
            {
                User self = await _self.GetInfoAsync();
                if (update.Message.NewChatMembers.Any(member => member.Id == self.Id))
                {
                    _logger.Debug("joined in group");
                    return _joined;
                }
            }

            _logger.Information("could not determine a message processor. this message will be ignored {@update}", update);
            return null;
        }
    }
}