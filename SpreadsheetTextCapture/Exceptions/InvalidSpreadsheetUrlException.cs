using System;

namespace SpreadsheetTextCapture.Exceptions
{
    public class InvalidSpreadsheetUrlException : Exception
    {
        public string SpreadsheetUrl { get; set; }

        public override string Message => "Spreadsheet url is not valid";
    }
}