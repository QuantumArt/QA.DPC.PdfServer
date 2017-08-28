using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace QA.DPC.PDFServer.Services.DataContract.HtmlGenerator
{
    [JsonObject]
    public class GenerateHtmlResponse
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }
        [JsonProperty(PropertyName = "relativePath")]
        public string RelativePath { get; set; }
        [JsonProperty(PropertyName = "error")]
        public GenerateHtmlError Error { get; set; }
    }
}
