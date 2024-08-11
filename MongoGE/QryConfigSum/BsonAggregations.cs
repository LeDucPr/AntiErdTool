using MongoDB.Bson;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Nodes;

namespace MongoGE.QryConfigSum
{
    public class BsonAggregations
    {

    }

    public class BsonAggregation : BsonDocument
    {
        protected BsonValue _key=null!;
        protected BsonValue _val=null!;
        public BsonAggregation() : base() { }
        public BsonAggregation(string key, BsonValue bsonValue) : base(key, bsonValue)
        {
            _key = key;
            _val = bsonValue;
        }
        public BsonAggregation(MongoOperator moKey, BsonValue bsonValue)
            : base(StringQuery.ToString(moKey), bsonValue)
        {
            _key = StringQuery.ToString(moKey);
            _val = bsonValue;
        }
        public BsonAggregation(MongoLookupKeyword mlkKey, BsonValue bsonValue)
            : base(StringQuery.ToString(mlkKey), bsonValue)
        {
            _key = StringQuery.ToString(mlkKey);
            _val = bsonValue;
        }
        public override string ToString() { return _key == null ? new BsonDocument().ToString() : new BsonDocument(_key.ToString(), _val).ToJson(); }
    }
}
