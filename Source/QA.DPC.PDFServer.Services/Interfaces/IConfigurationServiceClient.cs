using System.Threading.Tasks;

namespace QA.DPC.PDFServer.Services.Interfaces
{
    public interface IConfigurationServiceClient
    {
        Task<CustomerCodeConfiguration> GetCustomerCodeConfiguration(string customerCode);
        Task<string> GetCustomerCodeConfigurationJson(string customerCode);
    }
}