using Newtonsoft.Json;

namespace QA.DPC.PDFServer.Services.DataContract.HtmlGenerator
{
    [JsonObject]
    public class GenerateHtmlError
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}