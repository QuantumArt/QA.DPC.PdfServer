using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QA.DPC.PDFServer.Services
{
    public interface IHtmlGenerator
    {
        Task<string> GenerateHtml(int productId, string category);
    }
}
