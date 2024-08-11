using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using System.Diagnostics.Eventing.Reader;
using System.Reflection.PortableExecutable;

namespace MongoGE.Connections
{
    public class MongoCollectionController
    {
        private IMongoCollection<BsonDocument> _collection;
        public MongoCollectionController(IMongoCollection<BsonDocument> collection)
        {
            _collection = collection;
        }
        public async Task CreateBsons(List<BsonDocument> bsons)
        {
            try { await _collection.InsertManyAsync(bsons); }
            catch (Exception ex) { throw new Exception($"Không thể khởi tạo mới: {ex.Message}"); }
        }
        public Task CreateBsonAsync(BsonDocument bson)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await _collection.InsertOneAsync(bson);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Không thể khởi tạo mới: {ex.Message}");
                }
            });
        }

        public async Task CreateBson(BsonDocument bson)
        {
            try { await _collection.InsertOneAsync(bson); }
            catch (Exception ex) { throw new Exception($"Không thể khởi tạo mới: {ex.Message}"); }
        }
        /// <summary>
        /// Thực hiện tìm kiếm
        /// </summary>
        /// <param name="setKey">Được viết dưới dạng string và lọc theo Bson</param>
        /// <param name="setValue">Được viết dưới dạng string và lọc theo Bson</param>
        /// <param name="documentId">Mặc định khi sử dụng ObjectId thì bỏ qua otherKey</param>
        /// <param name="filterKey">Được viết dưới dạng string và lọc theo Bson</param>
        /// <param name="filterValue">Được viết dưới dạng string và lọc theo Bson</param>
        /// <returns></returns>
        public async Task UpdateSizeValueAsync(string setKey, string setValue, ObjectId? documentId = default, string filterKey = "", string filterValue = "")
        {
            FilterDefinition<BsonDocument> filter; // bộ lọc theo Id
            if (documentId != default)
                filter = Builders<BsonDocument>.Filter.Eq("_id", documentId);
            else if (!string.IsNullOrEmpty(filterKey))
                filter = Builders<BsonDocument>.Filter.Eq(filterKey, filterValue);
            else
                filter = FilterDefinition<BsonDocument>.Empty;
            var update = Builders<BsonDocument>.Update.Set(setKey, setValue);
            try
            {
                var result = await _collection.UpdateOneAsync(filter, update);
                if (result.ModifiedCount <= 0)
                    throw new Exception("Không có tài liệu nào được cập nhật.");
            }
            catch (Exception ex) { Console.WriteLine($"Lỗi khi cập nhật tài liệu: {ex.Message}"); }
        }
        /// <summary>
        /// Đây coi như là 1 bước xác nhận nếu bạn gọi nhầm phải yêu cầu xóa này
        /// </summary>
        /// <param name="setKey"></param>
        /// <param name="setValue"></param>
        /// <param name="documentId"></param>
        /// <param name="filterKey"></param>
        /// <param name="filterValue"></param>
        /// <returns></returns>
        public async Task DeleteDocumentAsync(string setKey, string setValue, ObjectId? documentId = default, string filterKey = "", string filterValue = "")
        {
            FilterDefinition<BsonDocument> filter; // bộ lọc theo Id hoặc theo điều kiện khác
            if (documentId != default)
                filter = Builders<BsonDocument>.Filter.Eq("_id", documentId);
            else if (!string.IsNullOrEmpty(filterKey) && !string.IsNullOrEmpty(setValue) && filterKey == setKey)
                filter = Builders<BsonDocument>.Filter.Eq(filterKey, setValue);
            else if (!string.IsNullOrEmpty(filterKey))
                filter = Builders<BsonDocument>.Filter.Eq(filterKey, filterValue);
            else
                filter = FilterDefinition<BsonDocument>.Empty;
            try
            {
                var result = await _collection.DeleteOneAsync(filter);
                if (result.DeletedCount <= 0)
                    throw new Exception("Không có tài liệu nào được xóa.");
            }
            catch (Exception ex) { Console.WriteLine($"Lỗi khi xóa tài liệu: {ex.Message}"); }
        }
        public async Task<List<BsonDocument>> FindBsons(Dictionary<string, string> findComponents)
        {
            if (findComponents == null || findComponents.Count == 0)
                return new List<BsonDocument>();
            var filter = new BsonDocument(findComponents);
            List<BsonDocument> documents = await _collection.Find(filter).ToListAsync();
            return documents;
        }
        public async Task Indexes(params (string FieldName, bool IsUnique)[] fields)
        {
            foreach (var field in fields)
            {
                var indexKeys = Builders<BsonDocument>.IndexKeys.Ascending(field.FieldName);
                var indexOptions = new CreateIndexOptions { Unique = field.IsUnique };
                var indexModel = new CreateIndexModel<BsonDocument>(indexKeys, indexOptions);
                await _collection.Indexes.CreateOneAsync(indexModel);
            }
        }
        public async Task<List<BsonDocument>> SortCollection(string[] fieldNames)
        {
            var sort = Builders<BsonDocument>.Sort.Ascending(fieldNames[0]);
            for (int i = 1; i < fieldNames.Length; i++)
                sort = sort.Ascending(fieldNames[i]);
            List<BsonDocument> sortedDocuments = await _collection.Find(new BsonDocument()).Sort(sort).ToListAsync();
            return sortedDocuments;
        }
        // tạo aggregation
        public async Task<List<BsonDocument>> AggregateBsons(List<BsonDocument> pipeline)
        {
            var aggregation = _collection.Aggregate<BsonDocument>(pipeline);
            List<BsonDocument> documents = await aggregation.ToListAsync();
            return documents;
        }
    }
}
