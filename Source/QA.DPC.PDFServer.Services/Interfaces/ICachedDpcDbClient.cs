using System.Threading.Tasks;

namespace QA.DPC.PDFServer.Services.Interfaces
{
    public interface ICachedDpcDbClient
    {
        Task<string> GetHighloadApiAuthToken(string customerCode);
        Task<string> GetHighloadApiAuthToken(CustomerCodeConfiguration configuration);
    }
}