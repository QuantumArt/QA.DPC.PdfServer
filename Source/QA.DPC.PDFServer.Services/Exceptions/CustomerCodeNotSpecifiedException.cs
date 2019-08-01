using System;

namespace QA.DPC.PDFServer.Services.Exceptions
{
    public class CustomerCodeNotSpecifiedException : Exception
    {
        public CustomerCodeNotSpecifiedException() : base()
        {
        }

        public CustomerCodeNotSpecifiedException(string message) : base(message)
        {
        }

        public CustomerCodeNotSpecifiedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}