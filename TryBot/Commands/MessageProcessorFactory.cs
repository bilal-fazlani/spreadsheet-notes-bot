using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TryBot.Commands
{
    public static class MessageProcessorFactory
    {
        static Help help = new Help();
        static Note note = new Note();
        static Start start = new Start();
        static Joined joined = new Joined();

        public static async Task<IMessageProcessor> GetMessageProcessorAsync(MessageEventArgs messageEventArgs)
        {
            if (messageEventArgs.Message.Type == MessageType.Text)
            {
                string message = messageEventArgs.Message.Text.Trim();
            
                Regex regex = new Regex(@"\/(note|help|start)(.*)", RegexOptions.IgnoreCase);

                Match match = regex.Match(message);

                if (match.Success)
                {
                    string commandName = match.Groups[1].Value;
                
                    switch (commandName.ToLower())
                    {
                        case "note":
                            return note;
                        case "start":
                            return start;
                        case "help":
                            return help;
                        default:
                            return null;
                    }
                }

                return help;
            }

            if (messageEventArgs.Message.Type == MessageType.ChatMembersAdded)
            {
                User self = await Self.GetInfoAsync();
                if (messageEventArgs.Message.NewChatMembers.Any(member => member.Id == self.Id))
                {
                    return joined;
                }
            }

            return null;
        }
    }
}