using MongoConsOrgzGE.Structures.Common;
using MongoDB.Bson;

namespace MongoConsOrgzGE.Structures.Entities
{
    /// <summary>
    /// Key sẽ lưu các Property/Field của đối tượng
    /// Value sẽ là đầu nối của các đối tượng 
    /// Nếu Value sẽ được biểu diễn dưới dạng string, 
    /// giá trị trong string được nối liên tiếp
    /// "Phone", new BsonDocument() { // (Collection) Phone trong Collection này chứa các Field định danh tiếng Việt và được map với các Field được đánh mã định danh 
    ///      { "Tên điện thoại", "_0" }, // Việc đánh số được thực hiện từ _0, giá trị nào đó là đối tượng không phải là Field thuần thì _ đánh dấu số thứ tự trong Field của nó 
    ///      { "Mã Series", "_1" },
    ///      {"Kích thước", new BsonDocument(){
    ///          { "_", "_2" },  // "_" biểu thị giá trị kế tiếp hiện tại là 2 
    ///          { "Chiều rộng", "_0" }, // "_0" là đối tượng con mới của "Kích thước"
    ///          { "Chiều dài", "_1" }
    ///      }}
    /// }
    /// </summary>
    public class PrimitiveDataComponent : PrimeComponent
    {
        /// <summary>
        /// Đây là đối tượng duy nhất được set Parent trong họ BasicComponent
        /// </summary>
        private BasicComponent _parent = null!;
        public BasicComponent Parent { set => _parent = IsPrimeComponent(value); }
        public PrimitiveDataComponent() : base() { }
        public PrimitiveDataComponent(BsonValue bsonDataKey, BsonValue bsonDataValue)
            : base(bsonDataKey, bsonDataValue) { }
        private static PrimeComponent IsPrimeComponent(BasicComponent vcp)
        {
            if (vcp is PrimeComponent)
                return (PrimeComponent)vcp;
            throw new Exception("Dạng PrimeComponent là bắt buộc");
        }
    }
    public static partial class BasicComponentExtension { }
}
