using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace SpreadsheetTextCapture
{
    public class ErrorReportingAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger _logger;

        public ErrorReportingAttribute(ILogger logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            _logger.Error(context.Exception, "Error");
            base.OnException(context);
        }
    }
}