using System;

namespace SpreadsheetTextCapture.Exceptions
{
    public class UnauthorizedChatException : Exception
    {
        public UnauthorizedChatException(string chatId)
        {
            ChatId = chatId;
        }
        
        public string ChatId { get; }

        public override string Message => "This chat is un-authorized";
    }
}