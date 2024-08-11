using MongoDB.Bson;

namespace MongoGE.QryConfigSum.Rules
{
    internal class Common
    {
        public static BsonValue CreateDefaultBsonValue(Type bsonValType)
        {
            if (bsonValType == typeof(BsonDocument))
                return new BsonDocument();
            else if (bsonValType == typeof(BsonArray))
                return new BsonArray();
            else if (bsonValType == typeof(BsonValue))
                return BsonNull.Value;
            else //????
                throw new ArgumentException("Unsupported BsonValue type", nameof(bsonValType));
        }
        public static void AddToBsonValue(BsonValue bsonValue, BsonValue bsonCondition)
        {
            if (bsonValue is BsonArray bsonArray)
                bsonArray.Add(bsonCondition);
            else if (bsonValue is BsonDocument bsonDocument)
                bsonDocument.Add(new BsonElement("condition", bsonCondition));
            else
                throw new InvalidOperationException("Unsupported BsonValue type for addition.");
        }
    }
}
