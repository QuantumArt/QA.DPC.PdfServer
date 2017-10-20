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
        private readonly PdfTemplateSelectorSettings _settings;

        public PdfTemplateSelector(IOptions<PdfTemplateSelectorSettings> settings, IDpcApiClient dpcApiClient)
        {
            _dpcApiClient = dpcApiClient;
            _settings = settings.Value;
        }

        public async Task<PdfTemplate> GetPdfTemplateForProduct(int productId, string category, SiteMode siteMode)
        {
            var fields = new List<string> {"Id"};
            fields.AddRange(_settings.PdfTemplateFields);
            var productJson = await _dpcApiClient.GetProductJson(productId, false, siteMode, fields.ToArray());
            return await FindPdfTemplateInJson(category, siteMode, productJson, _settings.PdfTemplateFields, false);
        }

       

        public async Task<PdfTemplate> GetPdfTemplateForRoaming(string countryCode, string category, bool isB2B, SiteMode siteMode)
        {
            var fields = new List<string>{"Id"};
            fields.AddRange(_settings.RoamingPdfTemplateFields);
            var query = new NameValueCollection{{"Alias", countryCode}};
            var productJson = await _dpcApiClient.GetProductJson("RoamingCountry", query, false, siteMode, fields.ToArray());
            return await FindPdfTemplateInJson(category, siteMode, productJson, _settings.RoamingPdfTemplateFields, true);
        }

        private async Task<PdfTemplate> FindPdfTemplateInJson(string category, SiteMode siteMode, string productJson, string[] pdfTemplateFields, bool jsonIsArray)
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
            var templates = await _dpcApiClient.GetProducts<PdfTemplate>("pdf", distinctTemplateIds, siteMode, new[] { "*" });
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
    }
}