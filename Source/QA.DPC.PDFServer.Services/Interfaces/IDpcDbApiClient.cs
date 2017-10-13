using System.Threading.Tasks;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;

namespace QA.DPC.PDFServer.Services.Interfaces
{
    public interface IDpcDbApiClient
    {
        Task<T> GetProduct<T>(int id, string serviceSlug);

        Task<PdfScriptMapper> GetPdfScriptMapper(int id);
    }
}
