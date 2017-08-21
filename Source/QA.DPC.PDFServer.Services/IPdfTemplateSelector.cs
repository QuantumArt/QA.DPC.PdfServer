using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QA.DPC.PDFServer.Services
{
    public interface IPdfTemplateSelector
    {
        Task<int?> GetPdfTemplateId(string productJson, string category);
    }
}
