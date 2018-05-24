using System.Text.RegularExpressions;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace TryBot
{
    public static class TextParser
    {
        public static string ParseArgs(MessageEventArgs messageEventArgs)
        {
            if (messageEventArgs.Message.Type != MessageType.Text) return null;
            
            string message = messageEventArgs.Message.Text.Trim();
            
            Regex regex = new Regex(@"\/(note|help|start)(.*)", RegexOptions.IgnoreCase);

            Match match = regex.Match(message);

            if (match.Success)
            {
                string text = match.Groups[2].Value;
                return text;
            }

            return message;
        } 
    }
}