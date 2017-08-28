using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace QA.DPC.PDFServer.Services.Exceptions
{
    public class GetProductJsonException : Exception
    {
        public GetProductJsonException() : base()
        {
        }

        public GetProductJsonException(string message) : base(message)
        {
        }

        public GetProductJsonException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected GetProductJsonException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
