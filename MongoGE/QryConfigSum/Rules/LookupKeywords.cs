using MongoDB.Bson;
using System.Text.RegularExpressions;
using M = MongoGE.QryConfigSum.MongoLookupKeyword;

namespace MongoGE.QryConfigSum.Rules
{
    file class LookupKeywordsConfig // chỉ cho phép trong file này
    {
        public static (List<M> LookupKeywordType, Type enumType, List<Type> dfltBsonValTypes, List<Type> BsonValAddTypes)
        Single = (
            new List<M> {
                M.From,
                M.LocalField,
                M.ForeignField,
                M.As
            },
            typeof(M),
            new List<Type> { typeof(BsonValue) },
            new List<Type> { }
        ),
        Let = (
            new List<M> {
                M.Let
            },
            typeof(M),
            new List<Type> { typeof(BsonDocument) },
            new List<Type> { }
        ),
        Pipeline = (
            new List<M> {
                M.Pipeline
            },
            typeof(M),
            new List<Type> { typeof(BsonArray) },
            new List<Type> { typeof(BsonDocument), typeof(BsonValue) }
        );
    }

    public class LookupKeywords : BsonAggregation
    {
        private MongoLookupKeyword _mlkKey;
        public LookupKeywords(MongoLookupKeyword mlkKey, BsonValue bsonCondition)
            : base(StringQuery.ToString(mlkKey), GetDefaultBsonValue(mlkKey))
        {
            _mlkKey = mlkKey;
            try
            {
                if (LookupKeywordsConfig.Single.LookupKeywordType.Contains(mlkKey))
                {
                    if (LookupKeywordsConfig.Single.dfltBsonValTypes.Contains(bsonCondition.GetType()))
                        _val = bsonCondition;
                }
                else if (LookupKeywordsConfig.Let.LookupKeywordType.Contains(mlkKey))
                    if (LookupKeywordsConfig.Let.dfltBsonValTypes.Contains(bsonCondition.GetType()))
                        _val = bsonCondition;
                else if (LookupKeywordsConfig.Pipeline.LookupKeywordType.Contains(mlkKey))
                    _val = bsonCondition;
                else
                    throw new ArgumentException($"Không hỗ trợ toán tử {nameof(mlkKey)} trong MongoLookupKeyword");
            }
            catch
            {
                _key = null!;
                _val = null!;
            }
        }

        // chỉ dành cho việc khởi tạo mặc định đầu tiên 
        private static BsonValue GetDefaultBsonValue(MongoLookupKeyword mlkKey)
        {
            if (LookupKeywordsConfig.Single.LookupKeywordType.Contains(mlkKey))
                return Common.CreateDefaultBsonValue(LookupKeywordsConfig.Single.dfltBsonValTypes[0]);
            else if (LookupKeywordsConfig.Let.LookupKeywordType.Contains(mlkKey))
                return Common.CreateDefaultBsonValue(LookupKeywordsConfig.Let.dfltBsonValTypes[0]);
            else if (LookupKeywordsConfig.Pipeline.LookupKeywordType.Contains(mlkKey))
                return Common.CreateDefaultBsonValue(LookupKeywordsConfig.Pipeline.dfltBsonValTypes[0]);
            else
                throw new ArgumentException($"Không hỗ trợ toán tử {nameof(mlkKey)} trong MongoLookupKeyword");
        }
    }
}
