using System;

namespace QA.DPC.PDFServer.Services.Exceptions
{
    public class RoamingPdfGenerationNotConfiguredException: Exception
    {
        public RoamingPdfGenerationNotConfiguredException() : base()
        {
        }

        public RoamingPdfGenerationNotConfiguredException(string message) : base(message)
        {
        }

        public RoamingPdfGenerationNotConfiguredException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}