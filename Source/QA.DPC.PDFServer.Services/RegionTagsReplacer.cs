using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.DPC.PDFServer.Services
{
    public class RegionTagsReplacer : IRegionTagsReplacer
    {
        private readonly IDpcApiClient _dpcApiClient;

        public RegionTagsReplacer(IDpcApiClient dpcApiClient)
        {
            _dpcApiClient = dpcApiClient;
        }

        public async Task<string> ReplaceTags(string input, int productId, int? regionId)
        {
            if (!regionId.HasValue)
                return input;

            var regionTags = await _dpcApiClient.GetRegionTags(productId);
            if (regionTags == null || regionTags.Length == 0)
                return input;

            
            var tags = regionTags.FirstOrDefault(x => x.ProductId == productId);
            if (tags?.RegionTagsArray == null || tags.RegionTagsArray.Length == 0)
                return input;

            var dictionary = new Dictionary<string, string>();
            foreach (var regionTag in tags.RegionTagsArray)
            {
                //[replacement]tag=ab[/replacement]
                var key = $"[replacement]tag={regionTag.Title}[/replacement]";
                if (dictionary.ContainsKey(key)) continue;
                var value = regionTag.Values.FirstOrDefault(x => x.RegionsId.Contains(regionId.Value));
                if (value != null)
                {
                    dictionary.Add(key, value.Value);
                    dictionary.Add(key.Replace("=", "&#x3D;"), value.Value);
                } ;
            }

            if (dictionary.Keys.Any())
            {
                foreach (var key in dictionary.Keys)
                    input = input.Replace(key, dictionary[key]);
            }

            return input;
        }
    }
}
