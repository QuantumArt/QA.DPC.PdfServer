using System;

namespace QA.DPC.PDFServer.Services.Exceptions
{
    public class GetHighloadApiTokenException: Exception
    {
        public GetHighloadApiTokenException() : base()
        {
        }

        public GetHighloadApiTokenException(string message) : base(message)
        {
        }

        public GetHighloadApiTokenException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}