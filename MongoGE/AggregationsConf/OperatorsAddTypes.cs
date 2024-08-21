using MongoDB.Bson;
using MongoGE.AggregationsConf.Enums;
using System.Text;
using System.Text.Json.Nodes;
using EOType = MongoGE.AggregationsConf.Enums.EOperatorTypes;

namespace MongoGE.AggregationsConf
{
    public class OperatorsAddTypes
    {
        private static readonly Lazy<Dictionary<EOType, BsonDocument>> _keyTypesConf =
            new Lazy<Dictionary<EOType, BsonDocument>>(LoadKeyTypesConf);

        public static Dictionary<EOType, BsonDocument> KeyTypesConf => _keyTypesConf.Value;

        private static Dictionary<EOType, BsonDocument> LoadKeyTypesConf()
        {
            return new Dictionary<EOType, BsonDocument>
            {
                { EOType.Comparison, CreateComparisonBsonDocument() },
                { EOType.Logical, CreateLogicalBsonDocument() },
                { EOType.Set, CreateSetBsonDocument() },
                { EOType.Element, CreateElementBsonDocument() },
                { EOType.Stage, CreateStageBsonDocument() }
            };
        }
        #region Khởi tạo các toán tử cơ bản và các khóa cho phép cấu hình trong toán tử
        private static BsonDocument CreateComparisonBsonDocument() // tạo document cho toán tử so sánh
        {
            var bsonDocument = new BsonDocument();
            var comparisonOperators = new Dictionary<EComparisonOperator, BsonValue>
            {
                { EComparisonOperator.Equal, new BsonArray { BsonType.String, BsonType.Int32, BsonType.Double, BsonType.Boolean, BsonType.DateTime, BsonType.Array, BsonType.Document } },
                { EComparisonOperator.NotEqual, new BsonArray { BsonType.String, BsonType.Int32, BsonType.Double, BsonType.Boolean, BsonType.DateTime, BsonType.Array, BsonType.Document } },
                { EComparisonOperator.GreaterThan, new BsonArray { BsonType.String, BsonType.Int32, BsonType.Double, BsonType.DateTime, BsonType.Array, BsonType.Document } },
                { EComparisonOperator.GreaterThanOrEqual, new BsonArray { BsonType.String, BsonType.Int32, BsonType.Double, BsonType.DateTime, BsonType.Array, BsonType.Document } },
                { EComparisonOperator.LessThan, new BsonArray { BsonType.String, BsonType.Int32, BsonType.Double, BsonType.DateTime, BsonType.Array, BsonType.Document } },
                { EComparisonOperator.LessThanOrEqual, new BsonArray { BsonType.String, BsonType.Int32, BsonType.Double, BsonType.DateTime, BsonType.Array, BsonType.Document } }
            };
            AddOperatorToBsonDocument<EComparisonOperator>(bsonDocument, comparisonOperators);
            return bsonDocument;
        }
        private static BsonDocument CreateLogicalBsonDocument() // tạo document cho toán tử logic
        {
            var bsonDocument = new BsonDocument();
            var logicalOperators = new Dictionary<ELogicalOperator, BsonValue>
            {
                { ELogicalOperator.And, BsonType.Array },
                { ELogicalOperator.Or, BsonType.Array },
                { ELogicalOperator.Not, BsonType.Array }
            };
            AddOperatorToBsonDocument<ELogicalOperator>(bsonDocument, logicalOperators);
            return bsonDocument;
        }
        private static BsonDocument CreateSetBsonDocument() // tạo document cho toán tử set
        {
            var bsonDocument = new BsonDocument();
            var setOperators = new Dictionary<ESetOperator, BsonValue>
            {
                { ESetOperator.In, BsonType.Array },
                { ESetOperator.NotIn, BsonType.Array },
                { ESetOperator.All, BsonType.Array },
                { ESetOperator.Size, BsonType.Int32 },
                { ESetOperator.Exists, BsonType.Boolean }
            };
            AddOperatorToBsonDocument<ESetOperator>(bsonDocument, setOperators);
            return bsonDocument;
        }
        private static BsonDocument CreateElementBsonDocument() // tạo document cho toán tử element
        {
            var bsonDocument = new BsonDocument();
            var elementOperators = new Dictionary<EElementOperator, BsonValue>
            {
                { EElementOperator.Regex, BsonType.String },
                { EElementOperator.Type, BsonType.String }
            };
            AddOperatorToBsonDocument<EElementOperator>(bsonDocument, elementOperators);
            return bsonDocument;
        }
        private static BsonDocument CreateStageBsonDocument() // tạo document cho toán tử stage
        {
            var bsonDocument = new BsonDocument();
            AddOperatorToBsonDocument<EStageOperator>(bsonDocument, new Dictionary<EStageOperator, BsonValue>
            {
                { EStageOperator.Match, BsonType.Document }, // lọc sử liệu 
                { EStageOperator.Project, new BsonDocument { // toán tử xuất 
                    { "include", BsonType.String },
                    { "exclude", BsonType.String }
                }},
                { EStageOperator.Group, new BsonDocument { // nhóm dữ liệu
                    { "_id", BsonType.String }, // nhóm theo trường
                    { "sum", BsonType.String }, // tổng
                    { "avg", BsonType.String } // trung bình
                }},
                { EStageOperator.Sort, new BsonDocument { // sắp xếp
                    { "field", BsonType.String },
                    { "order", BsonType.Int32 }
                }},
                { EStageOperator.Skip, BsonType.Int32 }, // bỏ qua số lượng
                { EStageOperator.Limit, BsonType.Int32 }, // đặt giới hạn số lượng 
                { EStageOperator.Unwind, BsonType.String }, // giải phóng mảng
                { EStageOperator.Lookup, new BsonDocument { // toán tử Join 
                    { "from", BsonType.String }, // bốc từ collection
                    { "let", BsonType.Document }, // biến truyền vào
                    { "pipeline", BsonType.Array }, // mảng các stage
                    { "localField", BsonType.String }, // khóa chính
                    { "foreignField", BsonType.String }, // khóa ngoại
                    { "as", BsonType.String } // alias
                }},
                { EStageOperator.AddFields, new BsonDocument { // thêm trường mới
                    { "newField", BsonType.String } // tên trường mới (sau này được đồng bộ cấu hình alias)
                }},
                { EStageOperator.ReplaceRoot, new BsonDocument { // thay thế root
                    { "newRoot", BsonType.String } // tên trường mới (sau này được đồng bộ cấu hình alias)
                }},
                { EStageOperator.Merge, new BsonDocument { // gộp dữ liệu
                    { "into", BsonType.String }, // tên collection
                    { "on", BsonType.String }, // điều kiện gộp
                    { "whenMatched", BsonType.String }, // hành động khi trùng
                    { "whenNotMatched", BsonType.String } // hành động khi không trùng
                }},
                { EStageOperator.Bucket, new BsonDocument { // phân loại dữ liệu
                    { "groupBy", BsonType.String }, // trường phân loại
                    { "boundaries", BsonType.Array }, // mảng giới hạn
                    { "default", BsonType.String } // giá trị mặc định
                }},
                { EStageOperator.Sample, new BsonDocument { // lấy mẫu
                    { "size", BsonType.Int32 } // số lượng
                }},
                { EStageOperator.Count, BsonType.String }, // đếm số lượng
                { EStageOperator.Facet, new BsonDocument { // tạo nhiều kết quả 
                    { "output", BsonType.Document }
                }},
                { EStageOperator.ConcatArrays, BsonType.Array } // nối mảng
            });
            return bsonDocument;
        }
        #endregion Khởi tạo các toán tử cơ bản và các khóa cho phép cấu hình trong toán tử
        private static void AddOperatorToBsonDocument<T>(BsonDocument bsonDocument, Dictionary<T, BsonValue> operatorValues) where T : Enum
        {
            foreach (var value in Enum.GetValues(typeof(T)))
            {
                var fieldInfo = typeof(T).GetField(value.ToString()!);
                var attribute = (OperatorAttribute)Attribute.GetCustomAttribute(fieldInfo!, typeof(OperatorAttribute))!;
                if (attribute != null && operatorValues.ContainsKey((T)value))
                    bsonDocument.Add(attribute.Operator, operatorValues[(T)value]);
            }
        }
    }

    public class OperatorHelper
    {
        private static readonly Lazy<Dictionary<EOType, Dictionary<Enum, string>>> _operatorTypes =
            new Lazy<Dictionary<EOType, Dictionary<Enum, string>>>(LoadOperatorTypes);
        /// <summary>
        /// Cấu hình toán tử dạng cây, thông qua các phân loại toán tử để gọi ra thành phần con trong đó 
        /// </summary>
        public static Dictionary<EOType, Dictionary<Enum, string>> OperatorTypes => _operatorTypes.Value;
        /// <summary>
        /// Trả về toán tử đã được cấu hình trước (gọi trực tiếp)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetOperator<T>(T enumValue) where T : Enum
        {
            try
            {
                var eoType = GetEOTypeFromEnum(enumValue);
                return OperatorTypes[eoType][enumValue];
            }
            catch { return "toán tử không hỗ trợ."; }
        }

        private static Dictionary<EOType, Dictionary<Enum, string>> LoadOperatorTypes()
        {
            var operators = new Dictionary<EOType, Dictionary<Enum, string>>();
            operators[EOType.Comparison] = GetOperatorsFromEnum<EComparisonOperator>();
            operators[EOType.Logical] = GetOperatorsFromEnum<ELogicalOperator>();
            operators[EOType.Set] = GetOperatorsFromEnum<ESetOperator>();
            operators[EOType.Element] = GetOperatorsFromEnum<EElementOperator>();
            operators[EOType.Stage] = GetOperatorsFromEnum<EStageOperator>();
            return operators;
        }

        /// <summary>
        /// Attribute để đánh dấu cấu hình toán tử 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static Dictionary<Enum, string> GetOperatorsFromEnum<T>() where T : Enum
        {
            var operatorDict = new Dictionary<Enum, string>();

            foreach (var value in Enum.GetValues(typeof(T)))
            {
                var fieldInfo = typeof(T).GetField(value.ToString()!);
                var attribute = (OperatorAttribute)Attribute.GetCustomAttribute(fieldInfo!, typeof(OperatorAttribute))!;
                if (attribute != null)
                    operatorDict[(Enum)value] = attribute.Operator; // Lấy toán tử từ attribute
            }
            return operatorDict;
        }

        private static EOType GetEOTypeFromEnum<T>(T enumValue) where T : Enum
        {
            if (typeof(T) == typeof(EComparisonOperator)) return EOType.Comparison;
            if (typeof(T) == typeof(ELogicalOperator)) return EOType.Logical;
            if (typeof(T) == typeof(ESetOperator)) return EOType.Set;
            if (typeof(T) == typeof(EElementOperator)) return EOType.Element;
            if (typeof(T) == typeof(EStageOperator)) return EOType.Stage;
            throw new ArgumentException("Unknown enum type");
        }
        private static Dictionary<Enum, BsonValue> GetOperatorsFromEnum<T>(BsonDocument bsonDocument) where T : Enum
        {
            var operatorDict = new Dictionary<Enum, BsonValue>();

            foreach (var value in Enum.GetValues(typeof(T)))
            {
                var fieldInfo = typeof(T).GetField(value.ToString()!);
                var attribute = (OperatorAttribute)Attribute.GetCustomAttribute(fieldInfo!, typeof(OperatorAttribute))!;
                if (attribute != null && bsonDocument.TryGetValue(attribute.Operator, out BsonValue bsonValue))
                {
                    operatorDict[(Enum)value] = bsonValue;
                }
            }
            return operatorDict;
        }

        public static Dictionary<StringBuilder, BsonValue> GetAddedValues<T>(T enumValue) where T : Enum
        {
            EOType eoType = GetEOTypeFromEnum(enumValue); // EOperatorTypes 
            var operatorDict = OperatorTypes[eoType]; // _operatorTypes.Value[eoType]
            if (!operatorDict.ContainsKey(enumValue)) throw new ArgumentException("Unknown enum value");
            if (operatorDict.TryGetValue(enumValue, out string? operatorAttb)) // check có toán tử 
            {
                var result = new Dictionary<StringBuilder, BsonValue>();
                var bsonDocument = OperatorsAddTypes.KeyTypesConf[eoType];
                if (bsonDocument.TryGetValue(operatorAttb, out BsonValue bsonValue))
                    FindBsonTypeRecursion(new StringBuilder(operatorAttb), bsonValue, ref result);
                return result;
            }
            return new Dictionary<StringBuilder, BsonValue>();
        }
        /// <summary>
        /// Resursion để tìm điều kiện cho các toán tử và các khóa có thể có trong toán tử 
        /// </summary>
        /// <param name="operatorAttb">Toán tử/tập con của toán tử</param>
        /// <param name="bsonValue"></param>
        /// <param name="result"></param>
        private static void FindBsonTypeRecursion(StringBuilder operatorAttb, BsonValue bsonValue, ref Dictionary<StringBuilder, BsonValue> result)
        {
            if (bsonValue is BsonArray bsonArray) // bsonType được map dưới dạng số định danh của BsonType 
                foreach (var value in bsonArray)
                {
                    if (value is BsonInt32 bsonInt32) // chuyển đổi ngược lại tương tự cho BsonType 
                    {
                        var bsonType = (BsonType)bsonInt32.Value;
                        result[new StringBuilder($"{operatorAttb}")] = new BsonString(bsonType.ToString());
                    }
                    else
                        result[new StringBuilder(operatorAttb.ToString())] = value;
                }
            else if (bsonValue is BsonDocument nestedDocument)
            {
                foreach (var element in nestedDocument.Elements)
                    FindBsonTypeRecursion(new StringBuilder($"{operatorAttb}.{element.Name}"), element.Value, ref result);
                //result[element.Name] = element.Value;
            }
            else if (bsonValue is BsonInt32 bsonInt32)
            {
                var bsonType = (BsonType)bsonInt32.Value;
                result[new StringBuilder(operatorAttb.ToString())] = new BsonString(bsonType.ToString());
            }
            else
                result[new StringBuilder(operatorAttb.ToString())] = bsonValue;
        }
    }
}
