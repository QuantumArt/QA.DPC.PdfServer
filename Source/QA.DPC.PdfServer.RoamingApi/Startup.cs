using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QA.DPC.PdfServer.RoamingApi.Interfaces;
using QA.DPC.PdfServer.RoamingApi.Services;
using QA.DPC.PDFServer.Services;

namespace QA.DPC.PdfServer.RoamingApi
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ImpactApiSettings>(configuration.GetSection("ImpactApi"));
            services.AddTransient<RoamingHtmlGenerator>();
            services.AddTransient<IRoamingJsonMapper, RoamingJsonMapper>();
            services.AddTransient<IImpactApiClient, ImpactApiClient>();
        }
    }
}