using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.DPC.PDFServer.Services.Exceptions
{
    public class ProductMappingException : Exception
    {
        public ProductMappingException() : base()
        {
        }

        public ProductMappingException(string message) : base(message)
        {
        }

        public ProductMappingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
