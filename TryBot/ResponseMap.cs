using System;
using System.Collections.Generic;

namespace TryBot
{
    public static class ResponseMap
    {
        private static Dictionary<string, string[]> data = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["thanks"] = new [] {"You're welcome", ":)", "No problem :)"}
        };

        public static string GetResponse(string text)
        {
            bool exists = data.TryGetValue(text, out string[] values);
            if (exists)
            {
                int max = values.Length;
                int index = new Random().Next(0, max);
                return values[index];
            }

            return null;
        }
    }
}