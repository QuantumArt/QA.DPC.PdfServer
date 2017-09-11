using Newtonsoft.Json;

namespace QA.DPC.PDFServer.Services.DataContract.DpcApi
{
    public class RegionTags
    {
        [JsonProperty(PropertyName = "RegionTags")]
        public RegionTag[] RegionTagsArray { get; set; }
        public int ProductId { get; set; }

    }
}