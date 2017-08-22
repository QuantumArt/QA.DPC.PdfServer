using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace QA.DPC.PDFServer.Services.DataContract.HtmlGenerator
{
    public class GenerateHtmlRequest
    {
        [JsonProperty(PropertyName = "tariffData")]
        public GenerateHtmlFileInfo TariffData { get; set; }
        [JsonProperty(PropertyName = "templateData")]
        public GenerateHtmlFileInfo TemplateData { get; set; }
        [JsonProperty(PropertyName = "mapperData")]
        public GenerateHtmlFileInfo MapperData { get; set; }
        [JsonProperty(PropertyName = "templateEngine")]
        public string TemplateEngine { get; set; }
    }
}
