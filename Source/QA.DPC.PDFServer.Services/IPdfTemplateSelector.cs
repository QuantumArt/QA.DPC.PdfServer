using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using QA.DPC.PDFServer.Services.DataContract;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;

namespace QA.DPC.PDFServer.Services
{
    public interface IPdfTemplateSelector
    {
        Task<PdfTemplate> GetPdfTemplate(int productId, string category);
    }
}
