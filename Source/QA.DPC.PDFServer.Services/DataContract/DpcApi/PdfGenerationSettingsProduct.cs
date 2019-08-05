using Newtonsoft.Json;

namespace QA.DPC.PDFServer.Services.DataContract.DpcApi
{
    public class PdfGenerationSettingsProduct: DpcProductBase
    {
        public string Alias { get; set; }
        [JsonProperty("Value")]
        public string ValueJson { get; set; }
        public bool IsDefault { get; set; }
    }
}