using Newtonsoft.Json;

namespace QA.DPC.PDFServer.Services.DataContract.HtmlGenerator
{
    public class GenerateHtmlFileInfo
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "timestamp")]
        public long Timestamp { get; set; }
        [JsonProperty(PropertyName = "downloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonProperty(PropertyName = "siteMode")]
        public string SiteMode { get; set; }
    }
}