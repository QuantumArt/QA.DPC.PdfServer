using Newtonsoft.Json;

namespace QA.DPC.PDFServer.Services.DataContract.DpcApi
{
    public class DbApiProductWrapper<T>
    {
        [JsonProperty(PropertyName = "product")]
        public T Product { get; set; }
    }
}
