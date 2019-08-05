using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;

namespace QA.DPC.PDFServer.Services.Interfaces
{
    public interface IImpactApiClient
    {
        string GetRoamingProductDownloadUrl(string impactApiBaseUrl, string countryCode, bool isB2B, SiteMode siteMode);
    }
}
