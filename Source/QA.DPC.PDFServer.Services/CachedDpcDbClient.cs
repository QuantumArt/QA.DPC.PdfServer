using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using QA.Core.Cache;
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DPC.PDFServer.Services.Settings;

namespace QA.DPC.PDFServer.Services
{
    public class CachedDpcDbClient : ICachedDpcDbClient
    {
        private readonly IDpcDbClient _dpcDbClient;
        private readonly IVersionedCacheProvider2 _cacheProvider;
        private CacheSettings _cacheSettings;

        public CachedDpcDbClient(IDpcDbClient dpcDbClient, IVersionedCacheProvider2 cacheProvider,
            IOptions<CacheSettings> cacheSettings)
        {
            _dpcDbClient = dpcDbClient;
            _cacheProvider = cacheProvider;
            _cacheSettings = cacheSettings.Value;
        }

        public Task<string> GetHighloadApiAuthToken(string customerCode)
        {
            var key = customerCode;
            var result = _cacheProvider.GetOrAdd(key,
                TimeSpan.FromSeconds(_cacheSettings.HighloadApiTokenCacheTimeoutSeconds),
                () => _dpcDbClient.GetHighloadApiAuthToken(customerCode));
            return result;
        }

        public Task<string> GetHighloadApiAuthToken(CustomerCodeConfiguration configuration)
        {
            var key = configuration.Name;
            var result = _cacheProvider.GetOrAdd(key,
                TimeSpan.FromSeconds(_cacheSettings.HighloadApiTokenCacheTimeoutSeconds),
                () => _dpcDbClient.GetHighloadApiAuthToken(configuration));
            return result;
        }
    }
}