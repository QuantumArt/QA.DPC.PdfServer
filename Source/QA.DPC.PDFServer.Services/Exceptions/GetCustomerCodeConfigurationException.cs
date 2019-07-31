using System;

namespace QA.DPC.PDFServer.Services.Exceptions
{
    public class GetCustomerCodeConfigurationException: Exception
    {
        public GetCustomerCodeConfigurationException() : base()
        {
        }

        public GetCustomerCodeConfigurationException(string message) : base(message)
        {
        }

        public GetCustomerCodeConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}