using Microsoft.Extensions.DependencyInjection;
using QA.DPC.PDFServer.Services;

namespace QA.DPC.PdfServer.RoamingApi
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<RoamingHtmlGenerator>();
            services.AddTransient<IRoamingJsonMapper, RoamingJsonMapper>();
            services.AddTransient<RoamingPdfGenerationSettingsProvider>();
        }
    }
}