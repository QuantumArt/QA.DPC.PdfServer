using QA.DPC.PdfServer.RoamingApi.Interfaces;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;

namespace QA.DPC.PdfServer.RoamingApi.Services
{
    public class ImpactApiClient : IImpactApiClient
    {
        public string GetRoamingProductDownloadUrl(string impactApiBaseUrl, string countryCode, bool isB2B, SiteMode siteMode) => $"{impactApiBaseUrl}/mnr/country/{countryCode}?isB2C={(!isB2B).ToString().ToLowerInvariant()}&calculateImpact=true";
    }
}
