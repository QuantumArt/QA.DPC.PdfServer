using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.DPC.PDFServer.Services.DataContract;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DPC.PDFServer.Services.Settings;

namespace QA.DPC.PDFServer.Services
{
    public class PdfTemplateSelector : IPdfTemplateSelector
    {
        private readonly IDpcApiClient _dpcApiClient;

        private readonly IPdfGenerationSettingsProvider _pdfGenerationSettingsProvider;
        private readonly PdfTemplateSelectorSettings _settings;

        public PdfTemplateSelector(IOptions<PdfTemplateSelectorSettings> settings, IDpcApiClient dpcApiClient, IPdfGenerationSettingsProvider pdfGenerationSettingsProvider)
        {
            _dpcApiClient = dpcApiClient;
            _pdfGenerationSettingsProvider = pdfGenerationSettingsProvider;
            _settings = settings.Value;
        }

        public async Task<PdfTemplate> GetPdfTemplateForProduct(string customerCode, int productId, string category, SiteMode siteMode)
        {
            var fields = new List<string> {"Id"};
            var pdfTemplateFields = await GetPdfTemplateFields(customerCode, siteMode);
            fields.AddRange(pdfTemplateFields);
            var productJson = await _dpcApiClient.GetProductJson(customerCode, productId, false, siteMode, fields.ToArray());
            return await FindPdfTemplateInJson(customerCode, category, siteMode, productJson, pdfTemplateFields, false);
        }

        public async Task<PdfTemplate> GetPdfTemplateForRoaming(string customerCode, string countryCode, string category, bool isB2B, SiteMode siteMode)
        {
            var fields = new List<string>{"Id"};

            var pdfTemplateFields = await GetRoamingPdfTemplateFields(customerCode, siteMode);
            fields.AddRange(pdfTemplateFields);
            var query = new NameValueCollection{{"Alias", countryCode}};
            var productJson = await _dpcApiClient.GetProductJson(customerCode, "RoamingCountry", query, false, siteMode, fields.ToArray());
            return await FindPdfTemplateInJson(customerCode, category, siteMode, productJson, pdfTemplateFields, true);
        }

        private async Task<PdfTemplate> FindPdfTemplateInJson(string customerCode, string category, SiteMode siteMode, string productJson, string[] pdfTemplateFields, bool jsonIsArray)
        {
            if (productJson == null)
                throw new GetProductJsonException();
            JToken jObj;
            if (!jsonIsArray)
            {
                jObj = JObject.Parse(productJson);
            }
            else
            {
                var jArr = JArray.Parse(productJson);
                jObj = jArr.First();
            }
            
            var templateSearchArray = new List<int[]>();

            for (var i = 0; i < pdfTemplateFields.Length; i++)
            {
                var jsonPath = $"$.{pdfTemplateFields[i]}[*].Id";
                var tokens = jObj.SelectTokens(jsonPath).Values<int>();
                var castedTokens = tokens as int[] ?? tokens.ToArray();
                if (castedTokens.Any())
                {
                    templateSearchArray.Add(castedTokens);
                }
            }
            if (!templateSearchArray.Any())
                throw new TemplateNotFoundException();

            var distinctTemplateIds = templateSearchArray.SelectMany(x => x).Distinct().ToArray();
            var templates = await _dpcApiClient.GetProducts<PdfTemplate>(customerCode, "pdf", distinctTemplateIds, siteMode, new[] { "*" });
            var dpcPdfTemplates = templates as IList<PdfTemplate> ?? templates.ToList();
            for (var i = 0; i < templateSearchArray.Count; i++)
            {
                var i1 = i;
                var matchedTemplate =
                    dpcPdfTemplates.FirstOrDefault(x => templateSearchArray[i1].Contains(x.Id) &&
                                                        x.PdfTemplateCategory.Alias.Equals(category,
                                                            StringComparison.InvariantCultureIgnoreCase));
                if (matchedTemplate != null)
                    return matchedTemplate;
            }


            return null;
        }
        
        private async Task<string[]> GetPdfTemplateFields(string customerCode, SiteMode siteMode)
        {
            var pdfGenerationSettings = await _pdfGenerationSettingsProvider.GetDefaultSettings(customerCode, siteMode);

            return pdfGenerationSettings?.PdfTemplateSelector?.PdfTemplateFields != null && pdfGenerationSettings.PdfTemplateSelector.PdfTemplateFields.Any()
                ? pdfGenerationSettings.PdfTemplateSelector.PdfTemplateFields
                : _settings.PdfTemplateFields;
        }
        
        private async Task<string[]> GetRoamingPdfTemplateFields(string customerCode, SiteMode siteMode)
        {
            var pdfGenerationSettings = await _pdfGenerationSettingsProvider.GetDefaultSettings(customerCode, siteMode);

            return pdfGenerationSettings?.PdfTemplateSelector?.RoamingPdfTemplateFields != null && pdfGenerationSettings.PdfTemplateSelector.RoamingPdfTemplateFields.Any()
                ? pdfGenerationSettings.PdfTemplateSelector.RoamingPdfTemplateFields
                : _settings.RoamingPdfTemplateFields;
        }
        
    }
}