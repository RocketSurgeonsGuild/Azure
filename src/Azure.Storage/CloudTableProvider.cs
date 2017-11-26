using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Rocket.Surgery.Azure.Storage
{
    public class CloudTableProvider
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, CloudTable> _cloudTables = new ConcurrentDictionary<string, CloudTable>();
        private volatile CloudStorageAccount _storageAccount;
        private volatile string _connectionString;
        public CloudTableProvider(IOptionsMonitor<AzureStorageSettings> optionsMonitor, ILogger logger = null)
            : this(optionsMonitor.CurrentValue, logger)
        {
            optionsMonitor.OnChange(Clear);
        }

        public CloudTableProvider(AzureStorageSettings settings, ILogger logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _storageAccount = CloudStorageAccount.Parse(_connectionString = settings.ConnectionString);
        }

        public async Task<CloudTable> Get(string name)
        {
            if (_cloudTables.TryGetValue(name, out var table))
            {
                _logger.LogDebug("Found cloud table client '{Name}' in cache", name);
                return table;
            }

            _logger.LogDebug("Getting cloud table client for '{Name}'", name);
            table = CreateClient(name);
            _cloudTables.TryAdd(name, table);

            _logger.LogDebug("Getting cloud table client for '{Name}'", name);
            await table.CreateIfNotExistsAsync();
            _logger.LogDebug("Created cloud table client for '{Name}'", name);

            return table;
        }

        private void Clear(AzureStorageSettings settings)
        {
            if (_connectionString == settings.ConnectionString) return;

            _logger.LogInformation("ConnectionString changed from '{OriginalConnectionString}' to '{NewConnectionString}'", _connectionString, settings.ConnectionString);
            _storageAccount = CloudStorageAccount.Parse(_connectionString = settings.ConnectionString);
            foreach (var table in _cloudTables)
            {
                var client = CreateClient(table.Key);
                _cloudTables.AddOrUpdate(table.Key, client, (a, b) => client);
            }
        }

        private CloudTable CreateClient(string name)
        {
            // Always create the table client, in the event that the storage account was offline when we try to connect.
            return _storageAccount.CreateCloudTableClient().GetTableReference(name);
        }
    }
}
