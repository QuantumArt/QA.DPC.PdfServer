using System;

namespace QA.DPC.PDFServer.Services.Exceptions
{
    public class PdfGenerationSettingsException: Exception
    {
        public PdfGenerationSettingsException() : base()
        {
        }

        public PdfGenerationSettingsException(string message) : base(message)
        {
        }

        public PdfGenerationSettingsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}