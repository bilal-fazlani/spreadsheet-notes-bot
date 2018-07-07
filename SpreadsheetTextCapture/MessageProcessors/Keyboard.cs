using System.Threading.Tasks;
using SpreadsheetTextCapture.StateManagement;
using Telegram.Bot.Types;

namespace SpreadsheetTextCapture.MessageProcessors
{
    public class Keyboard : IMessageProcessor
    {
        private readonly KeyboardManager _keyboardManager;

        public Keyboard(KeyboardManager keyboardManager)
        {
            _keyboardManager = keyboardManager;
        }
        
        public async Task ProcessMessageAsync(Update update)
        {
            if (update.Message?.Text.ToLower() == "/cancel")
            {
                await _keyboardManager.OnClear();
            }
            
            if (_keyboardManager.IsAwaitingUrl())
            {
                await _keyboardManager.SetSpreadsheetUrl(update.Message.Text.Trim());
            }
            
            await _keyboardManager.FireAsync(update.Message.Text.Trim());
        }
    }
}