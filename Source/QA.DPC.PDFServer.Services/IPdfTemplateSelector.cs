using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using QA.DPC.PDFServer.Services.DataContract;

namespace QA.DPC.PDFServer.Services
{
    public interface IPdfTemplateSelector
    {
        Task<DpcPdfTemplate> GetPdfTemplateId(int productId, string category);
    }
}
