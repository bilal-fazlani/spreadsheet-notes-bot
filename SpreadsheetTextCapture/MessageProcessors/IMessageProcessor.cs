using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace SpreadsheetTextCapture.MessageProcessors
{
    public interface IMessageProcessor
    {
        Task ProcessMessageAsync(Update update);
    }
}