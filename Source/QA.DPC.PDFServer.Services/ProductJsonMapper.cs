using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.DataContract.HtmlGenerator;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DPC.PDFServer.Services.Settings;


namespace QA.DPC.PDFServer.Services
{
    public class ProductJsonMapper : IProductJsonMapper
    {
        private readonly IPdfTemplateSelector _pdfTemplateSelector;
        private readonly IDpcApiClient _dpcApiClient;
        private readonly IDpcDbApiClient _dpcApiDbApiClient;
        private readonly NodeServerSettings _settings;

        public ProductJsonMapper(IOptions<NodeServerSettings> settings, IPdfTemplateSelector pdfTemplateSelector, IDpcApiClient dpcApiClient, IDpcDbApiClient dpcApiDbApiClient)
        {
            _pdfTemplateSelector = pdfTemplateSelector;
            _dpcApiClient = dpcApiClient;
            _dpcApiDbApiClient = dpcApiDbApiClient;
            _settings = settings.Value;
        }

        public async Task<string> MapProductJson(int productId, string category, int? mapperId, int? templateId, bool forceDownload, SiteMode siteMode)
        {
            
            if (!mapperId.HasValue)
            {
                PdfTemplate pdfTemplate;
                if (templateId.HasValue)
                {
                    pdfTemplate = await _dpcApiClient.GetProduct<PdfTemplate>(templateId.Value, siteMode);
                }
                else
                {
                    pdfTemplate = await _pdfTemplateSelector.GetPdfTemplate(productId, category, siteMode);
                }
                mapperId = pdfTemplate.PdfScriptMapper.Id;
            }

            var mapper = await _dpcApiDbApiClient.GetPdfScriptMapper(mapperId.Value);


            var productBase = await _dpcApiClient.GetProduct<DpcProductBase>(productId, false, siteMode, new[] { "Id", "UpdateDate" });
            if (productBase == null)
                throw new GetProductJsonException();
            var productDownloadUrl = _dpcApiClient.GetProductJsonDownloadUrl(productId, true, siteMode);

            var request = new PreviewJsonRequest
            {
                TariffData = new GenerateHtmlFileInfo
                {
                    Id = productId,
                    Timestamp = ConvertToTimestamp(productBase.UpdateDate),
                    ForceDownload = forceDownload,
                    DownloadUrl = productDownloadUrl,
                    SiteMode = siteMode.ToString()
                },

                MapperData = new GenerateHtmlFileInfo
                {
                    Id = mapperId.Value,
                    Timestamp = ConvertToTimestamp(mapper.Timestamp),
                    ForceDownload = forceDownload,
                    DownloadUrl = $"{_settings.DpcStaticFilesScheme}:{mapper.PdfScriptMapperFile.AbsoluteUrl}",
                    SiteMode = "db"
                },
            };

            var response = await MakeRequest(request);
            if (response.Success)
                return response.Json;

            throw new ProductMappingException(response.Error?.Message ?? "Unknown error while mapping product");
        }

        private async Task<PreviewJsonResponse> MakeRequest(PreviewJsonRequest request)
        {
            using (var client = new HttpClient())
            {
                var result = await client.PostAsync($"{_settings.GenerateBaseUrl}/previewJson",
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

                var stringResult = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<PreviewJsonResponse>(stringResult);
            }
        }

        private static long ConvertToTimestamp(DateTime date)
        {
            return ((DateTimeOffset)date).ToUnixTimeSeconds();
        }
    }
}
