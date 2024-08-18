using MongoDB.Bson;
using EOType = MongoGE.AggregationsConf.Enums.EOperatorTypes;

namespace MongoGE.AggregationsConf
{
    public class OperatorsAddTypes
    {
        private static Dictionary<EOType, BsonDocument> KeyTypesConf = new Dictionary<EOType, BsonDocument>
        {
            { EOType.Comparison, new BsonDocument { 
                { "$eq", new BsonArray { BsonType.String, BsonType.Int32, BsonType.Double, BsonType.Boolean, BsonType.DateTime, BsonType.Array, BsonType.Document} }, 
                { "$ne", new BsonArray { BsonType.String, BsonType.Int32, BsonType.Double, BsonType.Boolean, BsonType.DateTime, BsonType.Array, BsonType.Document} }, 
                { "$gt", new BsonArray { BsonType.String, BsonType.Int32, BsonType.Double, BsonType.DateTime, BsonType.Array, BsonType.Document} }, 
                { "$gte", new BsonArray { BsonType.String, BsonType.Int32, BsonType.Double, BsonType.DateTime, BsonType.Array, BsonType.Document} }, 
                { "$lt", new BsonArray { BsonType.String, BsonType.Int32, BsonType.Double, BsonType.DateTime, BsonType.Array, BsonType.Document} }, 
                { "$lte", new BsonArray { BsonType.String, BsonType.Int32, BsonType.Double, BsonType.DateTime, BsonType.Array, BsonType.Document} } }
            },
            { EOType.Logical, new BsonDocument { 
                { "$and", BsonType.Array }, { "$or", BsonType.Array }, { "$not", BsonType.Array } } },
            { EOType.Set, new BsonDocument { { "$in", BsonType.Array }, { "$nin", BsonType.Array }, { "$all", BsonType.Array }, { "$size", BsonType.Int32 }, { "$exists", BsonType.Boolean } } },
            { EOType.Element, new BsonDocument { { "$regex", BsonType.String }, { "$type", BsonType.String } } },
            { EOType.Stage, new BsonDocument { { "$match", BsonType.Document }, { "$project", BsonType.Document }, { "$group", BsonType.Document }, { "$sort", BsonType.Document }, { "$skip", BsonType.Int32 }, { "$limit", BsonType.Int32 }, { "$unwind", BsonType.String }, { "$lookup", BsonType.Document }, { "$addFields", BsonType.Document }, { "$replaceRoot", BsonType.Document }, { "$merge", BsonType.Document }, { "$bucket", BsonType.Document }, { "$sample", BsonType.Document }, { "$count", BsonType.String }, { "$facet", BsonType.Document }, { "$concatArrays", BsonType.Array } } }
        };
    }
}
