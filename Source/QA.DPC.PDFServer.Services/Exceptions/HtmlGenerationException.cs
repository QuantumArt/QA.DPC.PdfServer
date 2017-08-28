using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.DPC.PDFServer.Services.Exceptions
{
    public class HtmlGenerationException : Exception
    {
        public HtmlGenerationException() : base()
        {
        }

        public HtmlGenerationException(string message) : base(message)
        {
        }

        public HtmlGenerationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
