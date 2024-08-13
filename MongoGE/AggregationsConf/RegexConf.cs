using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MongoGE.AggregationsConf
{
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
}
