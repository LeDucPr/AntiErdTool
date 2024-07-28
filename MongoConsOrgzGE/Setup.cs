using MongoConsOrgzGE.Structures.Common;
using Cli = MongoGE.Connections.MongoClientController;
using Db = MongoGE.Connections.MongoDatabaseController;
using Collection = MongoGE.Connections.MongoCollectionController;
using MongoGE.Connections;

namespace MongoConsOrgzGE
{
    /// <summary>
    /// Class chứa các thực thi kết nối tới data nguồn và các thao tác trên MasterCollection 
    /// Các tham số chính chỉ được khởi tạo một lần duy nhất (bằng Lazy)
    /// </summary>
    public static class Setup
    {
        #region Thông số kết nối mặc định
        private static Lazy<Cli> _mongoCli = null!;
        private static Lazy<Db> _mongoDb = null!;//Cần tham số khởi tạo phù hợp 
        private static Lazy<Collection> _mongoMasterCollection = null!;//* Cần tham số khởi tạo phù hợp
        private static Lazy<string> _mongoDbSrv = new Lazy<string>("mongodb://localhost:27017");
        private static Lazy<string> _mongoDbName = new Lazy<string>("GeManagerTest");
        private static Lazy<string> _collectionMaster = new Lazy<string>("ViewMapper");
        private static Lazy<Boolean> _isCreateNew = null!;// = new Lazy<bool>(false);
        #endregion Thông số kết nối mặc định
        #region Khởi tạo kết nối 
        // Khởi tạo kết nối tới MongoDb Client 
        public static void InitializeMongoClient(string mongoDbSrv = null!)
        {
            if (_isCreateNew != null && !_isCreateNew.Value)
                return;
            _mongoDbSrv = new Lazy<string>(mongoDbSrv ?? _mongoDbSrv.Value);
            _mongoCli = new Lazy<Cli>(Cli.Instance(_mongoDbSrv.Value));
            _isCreateNew = null!; 
            _isCreateNew = new Lazy<Boolean>(true);
        }
        // Khởi tạo Database 
        public static void InitializeMongoDatabaseManager(string mongoDbName = null!)
        {
            if (_isCreateNew != null && !_isCreateNew.Value)
                return;
            InitializeMongoClient();
            try { _ = _mongoDb.Value[mongoDbName ?? _mongoDbName.Value]; }
            catch (NullReferenceException)
            {
                _mongoDbName = new Lazy<string>(mongoDbName ?? _mongoDbName.Value);
                _mongoCli.Value.AddDatabaseControllers(new List<string> { _mongoDbName.Value });
                _mongoDb = new Lazy<Db>(_mongoCli.Value[_mongoDbName.Value]);
            }
        }
        // Khởi tạo MasterCollection
        public static void InitializeMasterCollection(string masterViewMapperCollectionName = null!)
        {
            if (_isCreateNew != null && !_isCreateNew.Value)
                return;
            InitializeMongoDatabaseManager();
            try { _ = _mongoDb.Value[_collectionMaster.Value]; }
            catch (KeyNotFoundException)
            {
                _collectionMaster = new Lazy<string>(masterViewMapperCollectionName ?? _collectionMaster.Value);
                _mongoDb.Value.AddCollectionControllers(new List<string>() { _collectionMaster.Value });
                _mongoMasterCollection = new Lazy<Collection>(_mongoDb.Value[_collectionMaster.Value]);
            }
        }
        #endregion Khởi tạo kết nối
        /// <summary>
        /// Có cách khác đi từ Cli (tức là dùng hàm GetMongoClientController()) để lấy ra MongoClientController.
        /// Và sau đó lấy DatabaseController từ MongoClientController rồi Add, nhưng khá phức tạp nên xài cái này cho tiện 
        /// </summary>
        /// <param name="collectionName"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddCollection(string collectionName)
        {
            if (_mongoDb.Value == null)
                throw new InvalidOperationException("Chưa được khởi tạo kết nối tới DataBase, thực hiện InitializeMongoDatabaseManager để khởi tạo theo mặc định");
            if (string.IsNullOrEmpty(collectionName))
                throw new ArgumentNullException(nameof(collectionName), "Tên Collection không được để trống");
            try { _ = _mongoDb.Value[collectionName]; }
            catch (KeyNotFoundException)
            {
                _mongoDb.Value.AddCollectionControllers(new List<string>() { collectionName });
            }
        }
        public static void AddDocumentToMasterCollection(BasicComponent basicComponent)
        {
            if (_mongoMasterCollection.Value == null)
                throw new InvalidOperationException("Chưa được khởi tạo kết nối tới DataBase, thực hiện InitializeMongoDatabaseManager để khởi tạo theo mặc định");
            if (string.IsNullOrEmpty(basicComponent.ToString()))
                throw new ArgumentNullException(nameof(basicComponent), "Document không được để trống");
            // có Task thì nên xài Wait() để đồng bộ (ở đây CreateBson là Task)
            _mongoDb.Value[_collectionMaster.Value].CreateBson(basicComponent.ToBsonDocument()).Wait();
        }
        public static Cli GetMongoClientController()
        {
            if (_mongoCli.Value == null)
                throw new InvalidOperationException("MongoClientController chưa được khởi tạo");
            return _mongoCli.Value;
        }
        public static Db GetMongoDatabaseController()
        {
            if (_mongoDb.Value == null)
                throw new InvalidOperationException("MongoDatabaseController chưa được khởi tạo");
            return _mongoDb.Value;
        }
        public static Collection GetMasterCollection()
        {
            if (_mongoMasterCollection.Value == null)
                throw new InvalidOperationException("MasterCollection chưa được khởi tạo");
            return _mongoMasterCollection.Value;
        }
    }
}
