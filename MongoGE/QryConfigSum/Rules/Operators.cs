using MongoDB.Bson;
using System.Text.RegularExpressions;
using M = MongoGE.QryConfigSum.MongoOperator;

namespace MongoGE.QryConfigSum.Rules
{
    #region Cấu hình các toán tử và phân cấp 
    file class OperatorsConfig // chỉ cho phép trong file này
    {
        public static (List<M> operatorType, Type enumType, List<Type> dfltBsonValTypes, List<Type> BsonValAddTypes)
        OneVsOne = ( // 1 điều kiện duy nhất cho 1 field 
            new List<M> {
                M.Equal,
                M.NotEqual,
                M.GreaterThan,
                M.GreaterThanOrEqual,
                M.LessThan,
                M.LessThanOrEqual,
                M.Like,
                M.NotLike,
                M.Exists
            },
            typeof(M),
            new List<Type> { typeof(BsonValue), typeof(BsonDocument) },
            new List<Type> { }
        ),
        MulsVsMuls = ( // nhiều điều kiện cho 1 hoặc nhiều field 
            new List<M>
            {
                M.In,
                M.NotIn,
                M.And,
                M.Or
            },
            typeof(M),
            new List<Type> { typeof(BsonArray) },
            new List<Type> { typeof(BsonDocument), typeof(BsonValue) }
        );
    }

    public class RegexConfig
    {
        private bool _startWithCaret;
        private bool _endWithDollar;
        private List<string> _patterns;
        private RegexOptions _options;

        /// <summary>
        /// Dành riêng cho toán tử Like và NotLike (tức là $regex và $not)
        /// </summary>
        public RegexConfig()
        {
            _startWithCaret = false;
            _endWithDollar = false;
            _patterns = new List<string>();
            _options = RegexOptions.None;
        }

        public void EnableStartWithCaret(bool enable) { _startWithCaret = enable; }
        public void EnableEndWithDollar(bool enable) { _endWithDollar = enable; }
        public void AddPattern(string pattern) { _patterns.Add(pattern); }
        public void RemovePattern(string pattern) { _patterns.Remove(pattern); }
        public void ClearPatterns() { _patterns.Clear(); }

        public void EnableIgnoreCase(bool enable)
        {
            if (enable)
                _options |= RegexOptions.IgnoreCase;
            else
                _options &= ~RegexOptions.IgnoreCase;
        }

        public void EnableMultiline(bool enable)
        {
            if (enable)
                _options |= RegexOptions.Multiline;
            else
                _options &= ~RegexOptions.Multiline;
        }

        public void EnableSingleline(bool enable)
        {
            if (enable)
                _options |= RegexOptions.Singleline;
            else
                _options &= ~RegexOptions.Singleline;
        }

        public void EnableUnicode(bool enable)
        {
            if (enable)
                _options |= RegexOptions.CultureInvariant;
            else
                _options &= ~RegexOptions.CultureInvariant;
        }
        /// <summary>
        /// Cái này thì trong C#
        /// </summary>
        /// <returns></returns>
        public Regex CreateRegex()
        {
            string fullPattern = (_startWithCaret ? "^" : "") + string.Join("", _patterns) + (_endWithDollar ? "$" : "");
            return new Regex(fullPattern, _options);
        }
        /// <summary>
        /// Hàm nạp chồng này nhằm mục đích xuất ra cho MongoDB
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string optionsString = GetOptionsString();
            return (_startWithCaret ? "^" : "") + string.Join("", _patterns) + (_endWithDollar ? "$" : "") + optionsString;
        }
        private string GetOptionsString()
        {
            string options = "";
            if (_options.HasFlag(RegexOptions.IgnoreCase)) options += "i";
            if (_options.HasFlag(RegexOptions.Multiline)) options += "m";
            if (_options.HasFlag(RegexOptions.Singleline)) options += "s";
            if (_options.HasFlag(RegexOptions.CultureInvariant)) options += "u";
            return options.Length > 0 ? $"(?{options})" : "";
        }
        // Hàm tạo biểu thức chính
        public void AddMainPattern(string pattern) { _patterns.Add(pattern); }
        // Hàm tạo chỉ chứa số
        public void AddNumericPattern() { _patterns.Add(@"\d+"); }
        // Hàm tạo chỉ chứa chữ
        public void AddAlphabeticPattern() { _patterns.Add(@"[a-zA-Z]+"); }
        // Hàm tạo chỉ chứa (kiểu như toán tử in)
        public void AddInPattern(IEnumerable<string> values)
        {
            string pattern = string.Join("|", values);
            _patterns.Add($"({pattern})");
        }
        // Hàm tạo toán tử không chứa trong (not in)
        public void AddNotInPattern(IEnumerable<string> values)
        {
            string pattern = string.Join("|", values);
            _patterns.Add($"^(?!.*({pattern})).*$");
        }
        // Hàm tạo chỉ chứa ký tự đặc biệt
        public void AddSpecialCharacterPattern() { _patterns.Add(@"[\W_]+"); }
        // Hàm tạo chỉ chứa khoảng trắng
        public void AddWhitespacePattern() { _patterns.Add(@"\s+"); }
    }
    #endregion Cấu hình các toán tử và phân cấp 

    /// <summary>
    /// Bộ quản lý các thông số toán tử 
    /// </summary>
    public class Operators : BsonAggregation
    {
        //public BsonValue BsonValue { set => _bsonValue = value; }
        //var mo = (M)Enum.Parse(typeof(M), operatorType.ToString()); // chuyển T sang M 
        private MongoOperator _moKey;
        public Operators(MongoOperator moKey, BsonValue bsonCondition) 
            : base(StringQuery.ToString(moKey), GetDefaultBsonValue(moKey))
        {
            _moKey = moKey; // giá trị được đưa trực tiếp vào trong this 
            try
            {
                if (OperatorsConfig.OneVsOne.operatorType.Contains(moKey))
                {// nó chỉ có một điều kiên nên thay thế chứ không Add 
                    if (OperatorsConfig.OneVsOne.dfltBsonValTypes.Contains(bsonCondition.GetType()))
                        _val = bsonCondition;
                }
                else if (OperatorsConfig.MulsVsMuls.operatorType.Contains(moKey))
                {
                    if (OperatorsConfig.MulsVsMuls.dfltBsonValTypes.Contains(bsonCondition.GetType()))
                        _val = bsonCondition;
                    else if (OperatorsConfig.MulsVsMuls.BsonValAddTypes.Contains(bsonCondition.GetType()))
                        Common.AddToBsonValue(_val, bsonCondition);
                }
                else
                    throw new ArgumentException($"Không thể sử dụng toán tử {nameof(moKey)} trong Operators");
            }
            catch
            {
                _key = null!;
                _val = null!;
            }
        }
        private static BsonValue GetDefaultBsonValue(MongoOperator moKey)
        {
            if (OperatorsConfig.OneVsOne.operatorType.Contains(moKey))
                return Common.CreateDefaultBsonValue(OperatorsConfig.OneVsOne.dfltBsonValTypes[0]);
            else if (OperatorsConfig.MulsVsMuls.operatorType.Contains(moKey))
                return Common.CreateDefaultBsonValue(OperatorsConfig.MulsVsMuls.dfltBsonValTypes[0]);
            else
                throw new ArgumentException($"Không hỗ trợ toán tử {nameof(moKey)} trong MongoOperator");
        }
        private static MongoOperator ConvertToMongoOperator<T>(T operatorType)
        {
            if (!Enum.IsDefined(typeof(M), operatorType!))
                throw new ArgumentException("chỉ định dùng Type = MongoGE.QryConfigSum.MongoOperator");
            return (MongoOperator)Enum.Parse(typeof(M), operatorType!.ToString()!);
        }
    }
}
