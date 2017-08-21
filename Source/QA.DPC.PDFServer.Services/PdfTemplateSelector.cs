using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.DPC.PDFServer.Services.DataContract;
using QA.DPC.PDFServer.Services.Settings;

namespace QA.DPC.PDFServer.Services
{
    public class PdfTemplateSelector : IPdfTemplateSelector
    {
        private readonly IDpcApiClient _dpcApiClient;
        private PdfTemplateSelectorSettings _settings;

        public PdfTemplateSelector(IOptions<PdfTemplateSelectorSettings> settings, IDpcApiClient dpcApiClient)
        {
            _dpcApiClient = dpcApiClient;
            _settings = settings.Value;
        }

        public async Task<DpcPdfTemplate> GetPdfTemplateId(int productId, string category)
        {
            var fields = new List<string> {"Id"};
            fields.AddRange(_settings.PdfTemplateFields);
            var productJson = await _dpcApiClient.GetProductJson(productId, false, fields.ToArray());
            var jObj = JObject.Parse(productJson);
            var templateSearchArray = new List<int[]>();
            
            for (var i = 0; i < _settings.PdfTemplateFields.Length; i++)
            {
                var jsonPath = $"$.{_settings.PdfTemplateFields[i]}[*].Id";
                var tokens = jObj.SelectTokens(jsonPath).Values<int>();
                var castedTokens = tokens as int[] ?? tokens.ToArray();
                if (tokens != null && castedTokens.Any())
                {
                    templateSearchArray.Add(castedTokens);
                }
            }

            var distinctTemplateIds = templateSearchArray.SelectMany(x => x).Distinct().ToArray();
            var templates = await _dpcApiClient.GetProducts<DpcPdfTemplate>("pdf", distinctTemplateIds, new[] {"*"});
            var dpcPdfTemplates = templates as IList<DpcPdfTemplate> ?? templates.ToList();
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