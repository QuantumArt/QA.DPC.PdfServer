using System;

namespace QA.DPC.PDFServer.Services.Exceptions
{
    public class HighloadApiTokenMissingException: Exception
    {
        public HighloadApiTokenMissingException() : base()
        {
        }

        public HighloadApiTokenMissingException(string message) : base(message)
        {
        }

        public HighloadApiTokenMissingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}