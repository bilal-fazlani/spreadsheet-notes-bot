using System;

namespace TryBot
{
    public class Message
    {
        public Message(string name, string comments, string dateTime, string sender)
        {
            Name = name;
            Comments = comments;
            DateTime = dateTime;
            Sender = sender;
        }

        public string Name { get; set; }
        
        public string Comments { get; set; }

        public string DateTime { get; set; }

        public string  Sender { get; set; }
    }
}