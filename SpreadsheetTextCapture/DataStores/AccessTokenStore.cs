using System.Threading.Tasks;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Serilog;

namespace SpreadsheetTextCapture.DataStores
{
    public class AccessTokenStore : IDataStore
    {
        private readonly ILogger _logger;
        private readonly IMongoDatabase _database;
        private const string COLLECTION_NAME = "auth";
        
        public AccessTokenStore(IOptions<BotConfig> options, ILogger logger)
        {
            _logger = logger;
            BotConfig botConfig = options.Value;
            var client = new MongoClient(botConfig.MongoConnectionString);
            _database = client.GetDatabase(botConfig.MongoDatabaseName);
        }
        
        public async Task StoreAsync<T>(string key, T value)
        {
            var collection = _database.GetCollection<AuthRecord<T>>(COLLECTION_NAME);
            
            AuthRecord<T> authRecord = new AuthRecord<T>(value, key);

            var replaceOneResult = await collection.ReplaceOneAsync(
                doc => doc.Id == key,
                authRecord, 
                new UpdateOptions {IsUpsert = true});
                
            _logger.Debug("mongo upsert match count: {matchCount}", replaceOneResult.MatchedCount);
        }

        public async Task DeleteAsync<T>(string key)
        {
            var collection = _database.GetCollection<AuthRecord<T>>(COLLECTION_NAME);
            await collection.DeleteOneAsync(x => x.Id == key);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var collection = _database.GetCollection<AuthRecord<T>>(COLLECTION_NAME);
            var record = await collection.Find(x => x.Id == key).SingleOrDefaultAsync();
            return record != null ? record.Value : default;
        }

        public async Task ClearAsync()
        {
            await _database.DropCollectionAsync(COLLECTION_NAME);
        }
    }

    public class AuthRecord<T>
    {
        public AuthRecord()
        {
            
        }

        public AuthRecord(T value, string key)
        {
            Value = value;
            Id = key;
        }
        
        public string Id { get; set; }
        public T Value { get; set; }
    }
}