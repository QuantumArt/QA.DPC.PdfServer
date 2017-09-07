using System;
using System.Runtime.Serialization;

namespace QA.DPC.PDFServer.PdfGenerator
{
    public class PdfGenerationException : Exception
    {
        public PdfGenerationException()
        {
        }

        public PdfGenerationException(string message) : base(message)
        {
        }

        public PdfGenerationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PdfGenerationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        //public PdfGenerationException(Exception exception) : base(String.Empty, exception)
        //{
            
        //}
    }
}