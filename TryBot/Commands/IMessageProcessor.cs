using System.Threading.Tasks;
using Telegram.Bot.Args;

namespace TryBot.Commands
{
    public interface IMessageProcessor
    {
        Task ProcessMessage(MessageEventArgs messageEventArgs);
    }
}