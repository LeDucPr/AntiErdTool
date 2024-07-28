using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoGE.Connections
{
    public class MongoDatabaseController
    {
        private readonly IMongoDatabase _database;
        private Dictionary<string, MongoCollectionController> _collections = null!;
        public List<string> CollectionNames { get => _collections.Keys.ToList(); }
        public MongoDatabaseController(IMongoDatabase databases)
        {
            _database = databases;
            _collections = new Dictionary<string, MongoCollectionController>();
        }
        public MongoCollectionController this[string collectionKey]
        {
            get
            {
                if (_collections.TryGetValue(collectionKey, out MongoCollectionController? controller))
                    return controller;
                else
                    throw new KeyNotFoundException($"The collection key '{collectionKey}' was not found.");
            }
        }
        public void AddCollections(List<string> collectionStrings)
        {
            foreach (string collectionString in collectionStrings)
            {
                // Tạo collection (table) trong database (schema)
                IMongoCollection<BsonDocument> collection = _database.GetCollection<BsonDocument>(collectionString);
                _collections.Add(collectionString, new MongoCollectionController(collection));
            }
        }
        public async Task DropCollectionAsync(string collectionKey)
        {
            if (_collections.ContainsKey(collectionKey))
            {
                await _database.DropCollectionAsync(collectionKey);
                _collections.Remove(collectionKey);
            }
            else
                throw new KeyNotFoundException($"The collection key '{collectionKey}' was not found.");
        }

    }
    public static class MongoDatabaseControllerExtensions
    {
        public static MongoDatabaseController AddCollectionControllers(this MongoDatabaseController mgdb, List<string> collectionStrings)
        {
            mgdb.AddCollections(collectionStrings);
            return mgdb;
        }
    }
}
