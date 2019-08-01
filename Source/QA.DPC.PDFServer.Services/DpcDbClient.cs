using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Npgsql;
using QA.Core.Cache;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DPC.PDFServer.Services.Settings;

namespace QA.DPC.PDFServer.Services
{
    public class DpcDbClient : IDpcDbClient
    {
        private readonly IConfigurationServiceClient _configurationServiceClient;
        private readonly IVersionedCacheProvider2 _cacheProvider;
        private CacheSettings _cacheSettings;


        public DpcDbClient(IConfigurationServiceClient configurationServiceClient,
            IVersionedCacheProvider2 cacheProvider, IOptions<CacheSettings> cacheSettings)
        {
            _configurationServiceClient = configurationServiceClient;
            _cacheProvider = cacheProvider;
            _cacheSettings = cacheSettings.Value;
        }

        public async Task<string> GetHighloadApiAuthToken(string customerCode)
        {
            try
            {
                var configuration = await _configurationServiceClient.GetCustomerCodeConfiguration(customerCode);
                return await GetHighloadApiAuthToken(configuration);
            }
            catch (GetHighloadApiTokenException)
            {
                throw;
            }
            catch (GetCustomerCodeConfigurationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new GetHighloadApiTokenException("Error while getting highload api token", ex);
            }
        }


        public async Task<string> GetHighloadApiAuthToken(CustomerCodeConfiguration configuration)
        {
            try
            {
                using (var connection = CreateConnection(configuration))
                {
                    var contentId = await GetContentId(connection);
                    return await GetToken(connection, contentId);
                }
            }
            catch (Exception ex)
            {
                throw new GetHighloadApiTokenException("Error while getting highload api token", ex);
            }
        }


        public Task<string> GetCachedHighloadApiAuthToken(string customerCode)
        {
            var key = customerCode;
            var result = _cacheProvider.GetOrAdd(key,
                TimeSpan.FromSeconds(_cacheSettings.HighloadApiTokenCacheTimeoutSeconds),
                () => GetHighloadApiAuthToken(customerCode));
            return result;
        }

        public Task<string> GetCachedHighloadApiAuthToken(CustomerCodeConfiguration configuration)
        {
            var key = configuration.Name;
            var result = _cacheProvider.GetOrAdd(key,
                TimeSpan.FromSeconds(_cacheSettings.HighloadApiTokenCacheTimeoutSeconds),
                () => GetHighloadApiAuthToken(configuration));
            return result;
        }

        private static DbConnection CreateConnection(CustomerCodeConfiguration configuration)
        {
            switch (configuration.DbType)
            {
                case DatabaseType.SqlServer:
                    var sqlConnection = new SqlConnection(configuration.ConnectionString);
                    sqlConnection.Open();
                    return sqlConnection;
                case DatabaseType.Postgres:
                    var npgsqlConnection = new NpgsqlConnection(configuration.ConnectionString);
                    npgsqlConnection.Open();
                    return npgsqlConnection;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task<int?> GetContentId(DbConnection connection)
        {
            using (var cmd = CreateCommand(connection,
                "SELECT value from app_settings where key = 'HIGHLOAD_API_USERS_CONTENT_ID'"))
            {
                var result = await cmd.ExecuteScalarAsync();
                return result is DBNull ? (int?) null : Convert.ToInt32(result);
            }
        }

        private async Task<string> GetToken(DbConnection connection, int? highloadApiUsersContentId)
        {
            if (highloadApiUsersContentId == null)
                return null;

            using (var cmd = CreateCommand(connection, $@"
                        SELECT accesstoken 
                        from content_{highloadApiUsersContentId}_united 
                        where name = 'PdfGeneration' and archive = 0 and visible = 1
                        "))
            {
                var result = await cmd.ExecuteScalarAsync();
                return result is DBNull ? null : (string) result;
            }
        }

        private DbCommand CreateCommand(DbConnection connection, string query)
        {
            switch (connection)
            {
                case SqlConnection sqlConnection:
                    return new SqlCommand(query, sqlConnection);
                case NpgsqlConnection postgresConnection:
                    return new NpgsqlCommand(query, postgresConnection);
                default:
                    throw new ArgumentOutOfRangeException(nameof(connection), connection, null);
            }
        }
    }
}