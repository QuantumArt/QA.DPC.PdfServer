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
        protected  readonly IPdfTemplateSelector _pdfTemplateSelector;
        protected readonly IDpcApiClient _dpcApiClient;
        protected  readonly NodeServerSettings _settings;
        protected  readonly IHttpClientFactory _factory;


        public ProductJsonMapper(IOptions<NodeServerSettings> settings, IPdfTemplateSelector pdfTemplateSelector, IDpcApiClient dpcApiClient, IHttpClientFactory factory)
        {
            _pdfTemplateSelector = pdfTemplateSelector;
            _dpcApiClient = dpcApiClient;
            _settings = settings.Value;
            _factory = factory;
        }

        public async Task<string> MapProductJson(string customerCode, int productId, string category, int? mapperId, int? templateId, bool forceDownload, SiteMode siteMode)
        {
            
            if (!mapperId.HasValue)
            {
                PdfTemplate pdfTemplate;
                if (templateId.HasValue)
                {
                    pdfTemplate = await _dpcApiClient.GetProduct<PdfTemplate>(customerCode, templateId.Value, siteMode);
                }
                else
                {
                    pdfTemplate = await _pdfTemplateSelector.GetPdfTemplateForProduct(customerCode, productId, category, siteMode);
                }
                mapperId = pdfTemplate.PdfScriptMapper.Id;
            }

            var mapper = await _dpcApiClient.GetProduct<PdfScriptMapper>(customerCode, mapperId.Value, true, siteMode);
//            var mapper = await _dpcApiDbApiClient.GetPdfScriptMapper(mapperId.Value);


            var productBase = await _dpcApiClient.GetProduct<DpcProductBase>(customerCode, productId, false, siteMode, new[] { "Id", "UpdateDate" });
            if (productBase == null)
                throw new GetProductJsonException();
            var productDownloadUrl = _dpcApiClient.GetProductJsonDownloadUrl(customerCode, productId, true, siteMode);

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

        protected async Task<PreviewJsonResponse> MakeRequest(PreviewJsonRequest request)
        {
            var client = _factory.CreateClient();
            var result = await client.PostAsync($"{_settings.GenerateBaseUrl}/previewJson",
                new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            var stringResult = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PreviewJsonResponse>(stringResult);
        }

        protected static long ConvertToTimestamp(DateTime date)
        {
            return ((DateTimeOffset)date).ToUnixTimeSeconds();
        }
    }
}
