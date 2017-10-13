using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QA.DPC.PDFServer.Services.DataContract.HtmlGenerator
{
    [JsonObject]
    public class PreviewJsonResponse
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }
        [JsonProperty(PropertyName = "jsonString")]
        public string Json { get; set; }
        [JsonProperty(PropertyName = "error")]
        public GenerateHtmlError Error { get; set; }
    }
}
