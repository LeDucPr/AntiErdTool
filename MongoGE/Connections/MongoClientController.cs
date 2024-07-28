using MongoDB.Driver;

namespace MongoGE.Connections
{
    public class MongoClientController
    {
        private readonly IMongoClient _client = null!;
        private Dictionary<string, MongoDatabaseController> _databases = null!;
        public List<string> DatabaseNames { get => _databases.Keys.ToList(); }
        #region Instance
        private static MongoClientController? _instance;
        public static MongoClientController Instance(string mongoDbSrv)
        {
            if (_instance == null)
                _instance = new MongoClientController(mongoDbSrv);
            return _instance;
        }
        #endregion Instance
        public MongoClientController(string mongoDbSrv)
        {
            _client = new MongoClient(mongoDbSrv);
            _databases = new Dictionary<string, MongoDatabaseController>();
        }
        public MongoDatabaseController this[string databaseKey]
        {
            get
            {
                if (_databases.TryGetValue(databaseKey, out MongoDatabaseController? controller))
                    return controller;
                else
                    throw new KeyNotFoundException($"The database key '{databaseKey}' was not found.");
            }
        }
        public void CreateDatabase(string databaseString)
        {
        }
        public void AddDatabases(List<string> databaseStrings)
        {
            foreach (string databaseString in databaseStrings)
            {
                IMongoDatabase database = _client.GetDatabase(databaseString);
                _databases.Add(databaseString, new MongoDatabaseController(database));
            }
        }
        public async Task DropDatabaseAsync(string databaseKey)
        {
            if (_databases.ContainsKey(databaseKey))
            {
                await _client.DropDatabaseAsync(databaseKey);
                _databases.Remove(databaseKey);
            }
            else
                throw new KeyNotFoundException($"The database key '{databaseKey}' was not found.");
        }
    }
    public static class MongoClientControllerExtensions
    {
        public static MongoClientController AddDatabaseControllers(this MongoClientController mgc, List<string> databaseStrings)
        {
            mgc.AddDatabases(databaseStrings);
            return mgc;
        }
    }
}
