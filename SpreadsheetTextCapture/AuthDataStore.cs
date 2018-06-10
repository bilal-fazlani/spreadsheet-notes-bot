using System;
using System.Threading.Tasks;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace SpreadsheetTextCapture
{
    public class AuthDataStore : IDataStore
    {
        private readonly IMongoDatabase _database;
        
        public AuthDataStore(IOptions<BotConfig> options)
        {
            BotConfig botConfig = options.Value;
            var client = new MongoClient(botConfig.MongoConnectionString);
            _database = client.GetDatabase(botConfig.MongoDatabaseName);
        }
        
        public async Task StoreAsync<T>(string key, T value)
        {
            var collection = _database.GetCollection<AuthRecord<T>>("auth");
            
            AuthRecord<T> authRecord = new AuthRecord<T>(value, key);

            var replaceOneResult = await collection.ReplaceOneAsync(
                doc => doc.Id == key, 
                authRecord, 
                new UpdateOptions {IsUpsert = true});
                
            Console.WriteLine($"mongo upsert match count: {replaceOneResult.MatchedCount}");
        }

        public async Task DeleteAsync<T>(string key)
        {
            var collection = _database.GetCollection<AuthRecord<T>>("auth");
            await collection.DeleteOneAsync(x => x.Id == key);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var collection = _database.GetCollection<AuthRecord<T>>("auth");
            var record = await collection.Find(x => x.Id == key).SingleOrDefaultAsync();
            return record != null ? record.Value : default;
        }

        public async Task ClearAsync()
        {
            await _database.DropCollectionAsync("auth");
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