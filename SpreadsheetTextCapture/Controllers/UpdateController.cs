using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SpreadsheetTextCapture.MessageProcessors;
using Telegram.Bot.Types;

namespace SpreadsheetTextCapture.Controllers
{
    public class UpdateController : ControllerBase
    {
        private readonly MessageProcessorFactory _messageProcessorFactory;

        public UpdateController(MessageProcessorFactory messageProcessorFactory)
        {
            _messageProcessorFactory = messageProcessorFactory;
        }
        
        [HttpPost]
        [Route("/update")]
        public async Task<IActionResult> Post([FromBody]Update update)
        {
            IMessageProcessor messageProcessor = await _messageProcessorFactory.GetMessageProcessorAsync(update);
            await messageProcessor.ProcessMessageAsync(update);

            return Ok();
        }
    }
}