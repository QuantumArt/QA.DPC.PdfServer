using Newtonsoft.Json;

namespace QA.DPC.PDFServer.Services.DataContract.HtmlGenerator
{
    [JsonObject]
    public class PreviewJsonRequest
    {
        [JsonProperty(PropertyName = "tariffData")]
        public GenerateHtmlFileInfo TariffData { get; set; }

        [JsonProperty(PropertyName = "mapperData")]
        public GenerateHtmlFileInfo MapperData { get; set; }
    }
}
