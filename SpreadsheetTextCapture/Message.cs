using System.Collections.Generic;
using System.Linq;

namespace SpreadsheetTextCapture
{
    public class Message
    {
        public Message(string comment, string dateTime, string addedBy)
        {
            Comment = comment;
            DateTime = dateTime;
            AddedBy = addedBy;
            CreateTags();
        }
        
        public string Comment { get; }

        public string DateTime { get; }

        public string  AddedBy { get; }       
        
        public List<string> Tags { get; private set; }

        private void CreateTags()
        {
            List<string> tags = Enumerable.Range(0, 30).Select(x => "").ToList();
            
            int tagCount = 0;
            bool readingTag = false;
            
            for (int i = 0; i < Comment.Length; i++)
            {                
                if (Comment[i] == '#')
                {
                    tagCount++;
                    readingTag = true;
                    continue;
                }

                if (readingTag)
                {
                    tags[tagCount - 1] += Comment[i];
                }
                
                if (Comment[i] == ' ')
                {
                    readingTag = false;
                }
            }

            Tags = tags.Take(tagCount).ToList();
        }
    }
}