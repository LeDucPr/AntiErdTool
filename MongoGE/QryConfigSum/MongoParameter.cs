using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Security.Cryptography.X509Certificates;

namespace MongoGE.QryConfigSum
{
    /// <summary>
    /// Toán tử MongoDB
    /// </summary>
    public enum MongoOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        Like,
        NotLike,
        Exists,
        In,
        NotIn,
        And,
        Or
    }

    /// <summary>
    /// Khóa truy cập trong toán tử $lookup
    /// </summary>
    public enum MongoLookupKeyword
    {
        From,
        LocalField,
        ForeignField,
        Pipeline,
        Let,
        As
    }

    /// <summary>
    /// Các toán tử trong Aggregation
    /// </summary>
    public enum MongoAggregation
    {
        // các toán tử thông dụng 
        Match,
        Project,
        Group,
        Lookup,
        Unwind,
        ConcatArrays,
        Facet,
        Sort,
        Limit,
        Skip,
        Out,
        AddFields,
        Set,
        Unset,
        ReplaceRoot,
        Bucket,
    }

    /// <summary>
    /// Khi kết hợp các bộ toán tử với nhau thì nó sẽ được sử dụng làm key trong BsonDocument
    /// hỗ trợ trong việc tạo Agrragate, Find, Update, Delete
    /// </summary>
    public class StringQuery
    {
        /////// Toán tử điều kiện 
        public static string Equal = "$eq"; // toán tử so sánh bằng
        public static string NotEqual = "$ne"; // toán tử so sánh khác
        public static string GreaterThan = "$gt"; // toán tử so sánh lớn hơn
        public static string GreaterThanOrEqual = "$gte"; // toán tử so sánh lớn hơn hoặc bằng
        public static string LessThan = "$lt"; // toán tử so sánh nhỏ hơn
        public static string LessThanOrEqual = "$lte"; // toán tử so sánh nhỏ hơn hoặc bằng
        // chú ý thằng này hơi khác chút so với Sql
        public static string Like = "$regex"; // toán tử regex so sánh giống
        public static string NotLike = "$not"; // toán tử so sánh không giống
        public static string Exists = "$exists"; // toán tử so sánh không null
        public static string In = "$in"; // toán tử so sánh trong
        public static string NotIn = "$nin"; // toán tử so sánh không trong
        public static string And = "$and"; // toán tử so sánh và
        public static string Or = "$or"; // toán tử so sánh hoặc
        public override string ToString()
        {
            return "Toán tử truy cập (sử dụng hàm)";
        }
        public static string ToString(MongoOperator mo)
        {
            var fieldName = mo.ToString();
            var field = typeof(StringQuery).GetField(fieldName);
            if (field == null)
                throw new Exception("Toán tử không tồn tại");
            var value = field.GetValue(null);
            if (value == null)
                throw new Exception("Toán tử không tồn tại");
            return (string)value;
        }
        /////// Các khóa truy cập trong toán tử $lookup
        public static string From = "from"; // tên collection cần join
        public static string LocalField = "localField"; // trường trong collection hiện tại
        public static string ForeignField = "foreignField"; // trường trong collection cần join
        public static string Pipeline = "pipeline"; // mảng các bước xử lý
        public static string Let = "let"; // biến truyền vào
        public static string As = "as"; // tên biến kết quả
        public static string ToString(MongoLookupKeyword mlk)
        {
            var fieldName = mlk.ToString();
            var field = typeof(StringQuery).GetField(fieldName);
            if (field == null)
                throw new Exception("Khóa truy cập không tồn tại");
            var value = field.GetValue(null);
            if (value == null)
                throw new Exception("Khóa truy cập không tồn tại");
            return (string)value;
        }
    }
}
