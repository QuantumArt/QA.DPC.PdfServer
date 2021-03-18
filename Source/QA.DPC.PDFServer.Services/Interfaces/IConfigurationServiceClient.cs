using System.Threading.Tasks;
using QP.ConfigurationService.Models;

namespace QA.DPC.PDFServer.Services.Interfaces
{
    public interface IConfigurationServiceClient
    {
        Task<CustomerConfiguration> GetCustomerCodeConfiguration(string customerCode);
    }
}