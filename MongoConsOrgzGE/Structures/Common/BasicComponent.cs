using MongoConsOrgzGE.Structures.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace MongoConsOrgzGE.Structures.Common
{
    /// <summary>
    /// Kiến trúc chuẩn cho việc thiết lập cây phân chia dạng Json, cho phép thêm các lớp nhanh chóng bằng Bson <DynamicType>
    /// </summary>
    public class BasicComponent : BsonValue
    {
        #region private Class
        /// <summary>
        /// Có thể có nhiều đối tượng nếu lưu bằng string thì sẽ trùng nhau
        /// Tạo một đối tượng trung gian để giải quyết vấn đề này
        /// </summary>
        protected partial class FString
        {
            string _value;
            public override string ToString() => _value;
            public FString(string value) { _value = value; }
        }
        #endregion private Class
        #region Children
        private int _indexInLocalGraph = default!;
        // chỉ nên sử dùng thằng này sau khi cây(tree) đã hoàn thiện 
        public int IndexInLocalGraph { get => _indexInLocalGraph; }
        protected Dictionary<FString, int> _childrenkeys; // cái đầu là từ khóa, cái thứ 2 là vị trí 
        public List<Tuple<string, int>> Contains()
        {
            return _childrenkeys.Select(x => new Tuple<string, int>(x.Key.ToString(), x.Value)).ToList();
        }
        protected List<BasicComponent> _children;
        public List<BasicComponent> Children => _children;
        public override BasicComponent? this[string key]
        {
            get
            {
                if (this.IsHasKey(key))
                    return _children.FirstOrDefault(c => c.BsonKey.ToString()!.Equals(key));
                throw new KeyNotFoundException($"Không có phần tử nào có key là {key}");
            }
        }
        private bool IsHasKey(string key)
        {
            return _childrenkeys.Any(k => k.Key.ToString() == key);
        }
        #endregion Children
        #region Value
        private BsonValue _bsonKey = null!;
        private BsonValue _bsonValue = null!;
        public BsonValue BsonKey { get => _bsonKey; set => _bsonKey = value; }
        public BsonType BsonketType { get => _bsonKey.BsonType; }
        public BsonValue BsonValue { get => _bsonValue; set => _bsonValue = value; }
        public BsonType BsonValueType { get => _bsonValue.BsonType; }
        #endregion Value
        #region Overload
        public BasicComponent()
        {
            _bsonKey = default!;
            _bsonValue = default!;
            _childrenkeys = default!;
            _children = default!;
        }
        public BasicComponent(BsonValue bsonKey, BsonValue bsonValue)
        {
            _bsonKey = bsonKey;
            _bsonValue = bsonValue;
            _childrenkeys = new Dictionary<FString, int>();
            _children = new List<BasicComponent>();
        }
        public BasicComponent(string json)
        {
            _bsonKey = default!;
            _bsonValue = default!;
            _childrenkeys = default!;
            _children = default!;
            var netObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            var bsonDocument = netObject.ToBsonDocument();
        }
        public static BasicComponent CreateBasicComponentFromJson(string json)
        {
            var netObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            var bsonDocument = ConvertToBsonDocument(netObject!);
            BasicComponent bcp = default!;
            CreateBCPFromJsonRecursion(ref bcp, bsonDocument);
            return bcp;
        }
        private static void CreateBCPFromJsonRecursion(ref BasicComponent bcp, BsonDocument bsonDocument)
        {
            foreach (var elem in bsonDocument)
            {
                if (elem.Value is BsonDocument bsonDocumentValue)
                {
                    bcp = bcp ?? new BasicComponent(elem.Name, default!);
                    BasicComponent bcpChild = default!;
                    CreateBCPFromJsonRecursion(ref bcpChild, bsonDocumentValue);
                    bcpChild = new BasicComponent(elem.Name.ToString(), bsonDocumentValue);
                    bcp.Add(bcpChild);
                }
                else
                {
                    bcp = bcp ?? new BasicComponent(elem.Name, default!);
                    BasicComponent bcpChild = new BasicComponent(elem.Name, elem.Value);
                    bcp.Add(bcpChild);
                }
            }
        }
        private static BsonDocument ConvertToBsonDocument(Dictionary<string, object> netObject)
        {
            var bsonDocument = new BsonDocument();
            foreach (var kvp in netObject)
            {
                bsonDocument.Add(kvp.Key, ConvertToBsonValue(kvp.Value));
            }
            return bsonDocument;
        }
        private static BsonValue ConvertToBsonValue(object value)
        {
            switch (value)
            {
                case JObject jObject:
                    return ConvertToBsonDocument(jObject.ToObject<Dictionary<string, object>>()!);
                case JArray jArray:
                    return new BsonArray(jArray.ToObject<List<object>>()?.ConvertAll(ConvertToBsonValue));
                default:
                    return BsonValue.Create(value);
            }
        }
        #endregion Overload
        #region Thực hiện ghi đè để không lỗi (Override)
        public override BsonType BsonType => _bsonValue?.BsonType ?? BsonType.Null;
        public override int CompareTo(BsonValue other) { throw new NotImplementedException(); }
        public override bool Equals(object obj) { throw new NotImplementedException(); }
        public override int GetHashCode() { throw new NotImplementedException(); }
        public override string ToString()
        {
            return new BsonDocument().Add(_bsonKey.ToString(), _bsonValue).ToString();
        }
        public BsonDocument ToBsonDocument()
        {
            return new BsonDocument().Add(_bsonKey.ToString(), _bsonValue);
        }
        #endregion Thực hiện ghi đè để không lỗi (Override)
        #region Hàm thực thi chức năng 
        public virtual void Add(BasicComponent basicComponent)
        {
            //if (_bsonValue == default || _bsonValue == null)
            //        _bsonValue = new BsonDocument();
            //    _bsonValue.AsBsonDocument.Add(basicComponent.BsonKey.ToString(), basicComponent.BsonValue); // thêm vào BsonDocument
            if (_bsonValue == null || _bsonValue.IsBsonNull) // chỉ thực hiện Add được khi nó là BsonDocument 
                _bsonValue = new BsonDocument();
            // Kiểm tra xem _bsonValue có phải là một BsonDocument không.
            if (_bsonValue is BsonDocument bsonDocument)
            {
                bsonDocument.Add(basicComponent.BsonKey.ToString(), basicComponent.BsonValue); // nếu cố ép kiểu như comment trên thì lõi CastType
                _children = _children ?? new List<BasicComponent>();
                _children.Add(basicComponent); // thêm vào danh sách _children để sau còn mò được Key cho tìm kiếm
            }
            else
                throw new InvalidOperationException("Không thể thêm BasicComponent vào BsonValue nếu không phải là BsonDocument");
        }
        /// <summary>
        /// Chỉ xóa được nếu nó thành phần thuộc Key của cây gốc 
        /// </summary>
        /// <param name="basicComponent"></param>
        public virtual void Delete(BasicComponent basicComponent)
        {
            try
            {
                if (_bsonValue is BsonDocument bsonDocument)
                {
                    bsonDocument.Remove(basicComponent.BsonKey.ToString());
                    _children.Remove(basicComponent);
                }
                this.ReloadChildrenKeys();
            }
            catch/*(Exception ex) */{ throw new Exception("Không có phần tử này"); }
        }
        /// <summary>
        /// Tìm tất cả các chỉ mục Cây để lưu lại cho việc tìm kiếm theo chiều sâu 
        /// Hàm này chỉ nên gọi thực thi khi mà cây đã hoàn tất
        /// Tốt nhất là nên gọi vào cây gốc (cây khởi tạo)
        /// </summary>
        public void ReloadChildrenKeys()
        {
            this.ReloadChildrenKeysRecursive(this);
            this.ReloadBsonRecursive(this);
        }
        private void ReloadChildrenKeysRecursive(BasicComponent bcp, int inLoop = 0)
        {
            bcp._indexInLocalGraph = inLoop;
            bcp._childrenkeys.Clear();
            bcp._childrenkeys = new Dictionary<FString, int>();
            foreach (var bcpChild in bcp._children)
            {
                ReloadChildrenKeysRecursive(bcpChild, inLoop + 1);
                bcp._childrenkeys.Add(new FString(bcpChild.BsonKey.ToString() ?? default!), inLoop);
                // gộp của thằng chilren vào parent 
                bcpChild._childrenkeys.ToList().ForEach(x => bcp._childrenkeys.Add(x.Key, x.Value));
            }
        }
        private void ReloadBsonRecursive(BasicComponent bcp)
        {
            bcp._bsonValue = new BsonDocument();
            foreach (var bcpChild in bcp._children)
            {
                if (bcpChild._bsonValue is BsonDocument bsonDocument)
                    ReloadBsonRecursive(bcpChild);
                bcp._bsonValue.AsBsonDocument.Add(bcpChild.BsonKey.ToString(), bcpChild.BsonValue);
            }
        }
        /// <summary>
        /// keyPath ở đây không chứa BsonKey mà chỉ chứa từ phần tử con gần nó nhất trở đi 
        /// </summary>
        /// <param name="keyPaths"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public BasicComponent FindChild(List<string> keyPaths = default!)
        {
            if (keyPaths != default! && keyPaths.Count != 0 && this.IsHasKey(keyPaths.Last().Trim()))
            {
                BasicComponent bcp = this;
                foreach (var key in keyPaths)
                    bcp = bcp[key] ?? throw new KeyNotFoundException($"Không có phần tử nào có key là {key}");
                return bcp;
            }
            throw new KeyNotFoundException($"Không có phần tử nào có key là {keyPaths?.Last().Trim()}");
        }
        public List<List<string>> FindAll(string key)
        {
            List<List<string>> result = new List<List<string>>();
            var keysList = _childrenkeys.Select(kvp => kvp.Key).ToList();
            var valuesList = _childrenkeys.Select(kvp => kvp.Value).ToList();
            var findResult = new List<Tuple<FString, int, int>>();
            for (int i = 0; i < keysList.Count; i++)
                if (keysList[i].ToString() == key.ToString())
                    findResult.Add(new Tuple<FString, int, int>(keysList[i], valuesList[i], i));
            return this.FindCore(findResult);
        }
        public List<List<string>> FindAll(int value)
        {
            List<List<string>> result = new List<List<string>>();
            var keysList = _childrenkeys.Select(kvp => kvp.Key).ToList();
            var valuesList = _childrenkeys.Select(kvp => kvp.Value).ToList();
            var findResult = new List<Tuple<FString, int, int>>();
            for (int i = 0; i < valuesList.Count; i++)
                if (valuesList[i] == value)
                    findResult.Add(new Tuple<FString, int, int>(keysList[i], valuesList[i], i));
            return this.FindCore(findResult);
        }
        private List<List<string>> FindCore(List<Tuple<FString, int, int>> findResult)
        {
            List<List<string>> result = new List<List<string>>();
            var keysList = _childrenkeys.Select(kvp => kvp.Key).ToList();
            var valuesList = _childrenkeys.Select(kvp => kvp.Value).ToList();
            // <Fstring, treeIndex, DictIndex>
            List<List<Tuple<FString, int, int>>> backwardScanResults = new List<List<Tuple<FString, int, int>>>();
            foreach (var item in findResult)
            {
                var bsrs = new List<Tuple<FString, int, int>>() { item };
                int inloopValue = item.Item2;
                for (int i = item.Item3; i >= 0; i--) // vị trí trong List (item3)
                {
                    if (valuesList[i] == inloopValue - 1 && inloopValue - 1 >= 0)
                    {
                        bsrs.Insert(0, new Tuple<FString, int, int>(keysList[i], valuesList[i], i));
                        inloopValue--;
                    }
                }
                backwardScanResults.Add(bsrs);
            }
            foreach (var item in backwardScanResults)
            {
                List<string> resultItem = new List<string>();
                foreach (var item2 in item)
                    resultItem.Add(item2.Item1.ToString());
                result.Add(resultItem);
            }
            return result;
        }
        /// <summary>
        /// Thường thì cái này lấy key ra phục vụ cho việc Update
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<List<string>> KeyConcat(string childKeyName, int indexInGlobalGraph)
        {
            int maxIndexInGlobalGraph = _childrenkeys.Select(x => x.Value).Max();
            if (indexInGlobalGraph > maxIndexInGlobalGraph + 1) // giả sử Max 2 thì bạn chỉ được truyền vào 2 tối đa 3 là lỗi 
                throw new Exception("Ngoài phạm vi của cây(tree)");
            List<List<string>> allResults = new List<List<string>>();
            KeyConcatRecursion(this, ref allResults, new List<string>(), childKeyName, indexInGlobalGraph);
            allResults.ForEach(x => x.Insert(0, _bsonKey.ToString()!));
            return allResults;
        }
        private void KeyConcatRecursion(BasicComponent bcp, ref List<List<string>> allResults, List<string> result, string childKeyName, int indexInGlobalGraph)
        {
            if (bcp.BsonValue is BsonDocument bsonDocument)
            {
                foreach (var elem in bsonDocument)
                {
                    result.Add(elem.Name);
                    if (elem.Name.Equals(childKeyName) && bcp._indexInLocalGraph + 1 == indexInGlobalGraph) // +1 để ra child, vì đang ở parent 
                        allResults.Add(new List<string>(result));
                    else
                        KeyConcatRecursion(bcp[elem.Name]!, ref allResults, result, childKeyName, indexInGlobalGraph);
                    result.RemoveAt(result.Count - 1);
                }
            }
        }
        #endregion Hàm thực thi chức năng 
    }

    #region Extension
    public static partial class BasicComponentExtension { }
    #endregion Extension
}
