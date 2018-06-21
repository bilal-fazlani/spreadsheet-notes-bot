using System;
using Serilog;
using SpreadsheetTextCapture.StateManagement;

namespace SpreadsheetTextCapture
{
    class Program2
    {
        static string input = null;
        
        static void Main(string[] args)
        {
            ILogger logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            KeyboardManager keyboardManager = new KeyboardManager(logger);
            
            keyboardManager.PrintAvailableCommands();
            
            while (ReadInput() != "exit")
            {
                if (keyboardManager.IsAwaitingUrl())
                {
                    keyboardManager.SetSpreadsheetUrl(input);
                    continue;
                }
                keyboardManager.Fire(input);
                
                keyboardManager.PrintAvailableCommands();
            }

            Console.WriteLine("~END~");
        }

        static string ReadInput()
        {
            Console.Write("Please enter text: ");
            input = Console.ReadLine();
            return input;
        }
    }
}