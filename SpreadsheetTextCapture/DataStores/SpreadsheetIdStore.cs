using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using SpreadsheetTextCapture.Exceptions;

namespace SpreadsheetTextCapture.DataStores
{
    public class SpreadsheetIdStore
    {
        private readonly IMongoCollection<ChatSpreadsheetUrl> _collection;

        public SpreadsheetIdStore(IOptions<BotConfig> options)
        {
            var botConfig = options.Value;
            MongoClient client = new MongoClient(botConfig.MongoConnectionString);
            IMongoDatabase db = client.GetDatabase(botConfig.MongoDatabaseName);
            _collection = db.GetCollection<ChatSpreadsheetUrl>("chat_spreadsheetId");
        }
        
        public async Task<string> GetSpreadSheetIdAsync(string chatId)
        {
            var query = _collection.Find(x => chatId.Equals(x.Id));
            ChatSpreadsheetUrl chatSpreadsheetId = await query.FirstOrDefaultAsync();
            if (chatSpreadsheetId != null)
            {
                return chatSpreadsheetId.SpreadsheetId;
            }

            throw new SpreadSheetNotSetException(chatId);
        }

        public async Task SetSpreadSheetIdAsync(string chatId, string spreadsheetId)
        {
            FilterDefinition<ChatSpreadsheetUrl> filter = Builders<ChatSpreadsheetUrl>.Filter.Eq(x => x.Id, chatId);

            await _collection.ReplaceOneAsync(filter, new ChatSpreadsheetUrl
            {
                SpreadsheetId = spreadsheetId,
                Id = chatId,
                LastModified = DateTimeOffset.Now
            }, new UpdateOptions
            {
                IsUpsert = true
            });
        }

        public string ConvertSpreadSheetIdToUrl(string spreadsheetId)
        {
            return $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/edit";
        }
    }

    public class ChatSpreadsheetUrl
    {
        /// <summary>
        /// Chat Id
        /// </summary>
        public string Id { get; set; }
        public string SpreadsheetId { get; set; }
        [BsonRepresentation(BsonType.String)]
        public DateTimeOffset LastModified { get; set; }
    }
}