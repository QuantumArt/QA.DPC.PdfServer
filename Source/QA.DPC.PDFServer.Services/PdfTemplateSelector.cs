using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.DPC.PDFServer.Services.Settings;

namespace QA.DPC.PDFServer.Services
{
    public class PdfTemplateSelector : IPdfTemplateSelector
    {
        private PdfTemplateSelectorSettings _settings;

        public PdfTemplateSelector(IOptions<PdfTemplateSelectorSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<int?> GetPdfTemplateId(string productJson, string category)
        {
            throw new NotImplementedException();

            var templateIdsByQueries = new int[_settings.PdfTemplateQueries.Length];
            var obj = JObject.Parse(productJson);
            for (var i = 0; i < _settings.PdfTemplateQueries.Length; i++)
            {
                var token = obj.SelectToken(_settings.PdfTemplateQueries[i]);
                if (token != null)
                {
                    
                }
            }
        }
    }
}