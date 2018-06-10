using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private readonly Self _self;

        public MessageProcessorFactory(Note note, SetSpreadsheet setSpreadsheet, Authorize authorize,
            Start start, Help help, Joined joined, Self self)
        {
            _note = note;
            _setSpreadsheet = setSpreadsheet;
            _authorize = authorize;
            _start = start;
            _help = help;
            _joined = joined;
            _self = self;
        }

        public async Task<IMessageProcessor> GetMessageProcessorAsync(Update update)
        {
            if (update.Message.Type == MessageType.Text)
            {
                string message = update.Message.Text.Trim();
            
                Regex regex = new Regex(@"\/(note|help|start|authorize|spreadsheet)(.*)", RegexOptions.IgnoreCase);

                Match match = regex.Match(message);

                if (match.Success)
                {
                    string commandName = match.Groups[1].Value;
                
                    switch (commandName.ToLower())
                    {
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
                    return _joined;
                }
            }

            return null;
        }
    }
}