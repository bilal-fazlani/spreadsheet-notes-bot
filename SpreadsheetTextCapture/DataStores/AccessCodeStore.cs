using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace SpreadsheetTextCapture.DataStores
{
    public class AccessCodeStore
    {
        private readonly IMongoCollection<ChatAccessCode> _collection;

        public AccessCodeStore(IOptions<BotConfig> options)
        {
            var botConfig = options.Value;
            MongoClient client = new MongoClient(botConfig.MongoConnectionString);
            IMongoDatabase db = client.GetDatabase(botConfig.MongoDatabaseName);
            _collection = db.GetCollection<ChatAccessCode>("chat_accessCode");
        }
        
        public async Task<string> GetCodeAsync(string chatId)
        {
            var query = _collection.Find(x => chatId.Equals(x.Id));
            ChatAccessCode chatAccessCode = await query.FirstOrDefaultAsync();
            return chatAccessCode?.AccessCode;
        }

        public async Task SetAccessCodeAsync(string chatId, string accessCode)
        {
            FilterDefinition<ChatAccessCode> filter = Builders<ChatAccessCode>.Filter.Eq(x => x.Id, chatId);

            await _collection.ReplaceOneAsync(filter, new ChatAccessCode
            {
                AccessCode = accessCode,
                Id = chatId,
                LastModified = DateTimeOffset.Now
            }, new UpdateOptions
            {
                IsUpsert = true
            });
        }
        
        public async Task DeleteCodeAsync(string chatId)
        {
            FilterDefinition<ChatAccessCode> filter = Builders<ChatAccessCode>.Filter.Eq(x => x.Id, chatId);
            await _collection.DeleteOneAsync(filter, CancellationToken.None);
        }
    }

    public class ChatAccessCode
    {
        /// <summary>
        /// Chat Id
        /// </summary>
        public string Id { get; set; }
        public string AccessCode { get; set; }
        [BsonRepresentation(BsonType.String)]
        public DateTimeOffset LastModified { get; set; }
    }
}