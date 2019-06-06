using Microsoft.Extensions.Options;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DPC.PDFServer.Services.Settings;

namespace QA.DPC.PDFServer.Services
{
    public class ImpactApiClient : IImpactApiClient
    {
        private readonly ImpactApiSettings _settings;

        public ImpactApiClient(IOptions<ImpactApiSettings> settings)
        {
            _settings = settings.Value;
        }

        public string GetRoamingProductDownloadUrl(string countryCode, bool isB2B, SiteMode siteMode) => $"{_settings.BaseUrl}/mnr/country/{countryCode}?isB2C={(!isB2B).ToString().ToLowerInvariant()}&calculateImpact=true";
    }
}
