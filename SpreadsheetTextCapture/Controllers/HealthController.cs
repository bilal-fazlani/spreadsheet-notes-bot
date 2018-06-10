using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace SpreadsheetTextCapture.Controllers
{
    public class HealthController : ControllerBase
    {        
        [Route("/health")]
        public IActionResult Health()
        {
            return Ok("OK");
        }
    }
}