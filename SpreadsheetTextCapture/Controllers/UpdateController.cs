using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SpreadsheetTextCapture.MessageProcessors;
using Telegram.Bot.Types;

namespace SpreadsheetTextCapture.Controllers
{
    public class UpdateController : ControllerBase
    {
        private readonly MessageProcessorFactory _messageProcessorFactory;
        private readonly ILogger _logger;

        public UpdateController(MessageProcessorFactory messageProcessorFactory, ILogger logger)
        {
            _messageProcessorFactory = messageProcessorFactory;
            _logger = logger;
        }
        
        [HttpPost]
        [Route("/update")]
        public async Task<IActionResult> Post([FromBody]Update update)
        {
            Console.WriteLine("-----------------------------------------------------------");
            _logger.Debug("update received for chat {chatId}", 
                update?.Message?.Chat?.Id ?? update?.EditedMessage?.Chat?.Id ?? update?.CallbackQuery?.Message?.Chat?.Id);
            Console.WriteLine("-----------------------------------------------------------");
            
            IMessageProcessor messageProcessor = await _messageProcessorFactory.GetMessageProcessorAsync(update);
            if(messageProcessor != null) 
                await messageProcessor.ProcessMessageAsync(update);

            return Ok();
        }
    }
}