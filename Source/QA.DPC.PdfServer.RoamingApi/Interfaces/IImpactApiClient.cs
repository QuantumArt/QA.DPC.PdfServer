using QA.DPC.PDFServer.Services.DataContract.DpcApi;

namespace QA.DPC.PdfServer.RoamingApi.Interfaces
{
    public interface IImpactApiClient
    {
        string GetRoamingProductDownloadUrl(string impactApiBaseUrl, string countryCode, bool isB2B, SiteMode siteMode);
    }
}
