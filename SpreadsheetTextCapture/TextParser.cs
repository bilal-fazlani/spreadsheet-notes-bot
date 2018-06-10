using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SpreadsheetTextCapture
{
    public class TextParser
    {
        private readonly ILogger _logger;

        public TextParser(ILogger logger)
        {
            _logger = logger;
        }
        
        public string ParseArgs(Update update)
        {
            if (update.Message.Type != MessageType.Text) return null;
            
            string message = update.Message.Text.Trim();
            
            _logger.Debug("parsing command arguments...");
            
            Regex regex = new Regex(@"\/(note|help|start|spreadsheet)(.*)", RegexOptions.IgnoreCase);
            
            Match match = regex.Match(message);

            if (match.Success)
            {   
                string text = match.Groups[2].Value;
                _logger.Debug("command arguments are: {args}", text.Trim());
                return text.Trim();
            }

            _logger.Debug("command arguments are: {args}", message);
            return message;
        }

        public Dictionary<string,string> ParseCallbackState(string text)
        {
            Dictionary<string, string> set = text.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Select(x => x.Split('=', 2, StringSplitOptions.RemoveEmptyEntries))
                .ToDictionary(x=>x[0], y=>y[1]);

            _logger.Debug("parsed state from google auth callback. state - {@keys}", set.Keys);
            
            return set;
        }
    }
}