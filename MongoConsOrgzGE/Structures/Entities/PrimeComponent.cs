using MongoConsOrgzGE.Structures.Common;
using MongoDB.Bson;

namespace MongoConsOrgzGE.Structures.Entities
{
    public enum PrimitiveType
    {
        Null, // Cái này dành cho lúc bạn kế thừa
        Int,
        Double,
        String,
        DateTime,
        Array,
    }
    /// <summary>
    /// Thực thể lưu trữ các giá trị cuối cùng, là giá trị mà tại đó, 
    /// các đối tượng được lưu trữ dưới dạng giá trị nguyên thủy
    /// Và tất nhiên rồi, chỉ String mới dùng làm Key được 
    /// </summary>
    public class PrimeComponent : EntityComponent
    {
        public PrimitiveType PrimeType { get; private set; }
        /// <summary>
        /// Nói cách khác là bạn cần tạo nó bằng cách kế thừa nếu theo cách nguyên thủy nhất
        /// Điều này để tránh khởi tạo mà không đi kèm kiểu
        /// </summary>
        /// <exception cref="Exception"></exception>
        public PrimeComponent()
        {
            if (this.GetType() == typeof(PrimeComponent))
                throw new Exception("Không thể khởi tạo PrimeComponent theo cách này từ chính nó");
            PrimeType = PrimitiveType.Null;
        }
        /// <summary>
        /// Chỉ PrimeComponent mới có thể khởi tạo theo cách này
        /// Đơn giản là dù có phải Array không thì BsonKey vẫn lưu kiểu nguyên thủy nếu tồn tại, 
        /// Còn nếu là Array thì BsonValue lưu nó là Array (PrimitiveType.Array)
        /// </summary>
        /// <param name="primeType"></param>
        public PrimeComponent(Type primeType)
            : base(ToStringPrimitiveType(MapTypeToPrimitiveType(primeType)),
                  primeType.IsArray ? ToStringPrimitiveType(PrimitiveType.Array) : string.Empty)
        {
            if (this.GetType() != typeof(PrimeComponent))
                throw new Exception("Chỉ PrimeComponent mới có thể khởi tạo theo cách này");
            PrimeType = MapTypeToPrimitiveType(primeType);
        }
        /// <summary>
        /// Cái này cũng chỉ dành cho việc khởi tạo đối tượng kế thừa của nó 
        /// </summary>
        /// <param name="bsonDataKey"></param>
        /// <param name="bsonDataValue"></param>
        /// <exception cref="Exception"></exception>
        public PrimeComponent(BsonValue bsonDataKey, BsonValue bsonDataValue)
            : base(bsonDataKey, bsonDataValue)
        {
            if (this.GetType() == typeof(PrimeComponent))
                throw new Exception("Không thể khởi tạo PrimeComponent theo cách này từ chính nó");
            PrimeType = PrimitiveType.Null;
        }
        private static PrimitiveType MapTypeToPrimitiveType(Type type)
        {
            if (type == typeof(int))
                return PrimitiveType.Int;
            else if (type == typeof(double))
                return PrimitiveType.Double;
            else if (type == typeof(string))
                return PrimitiveType.String;
            else if (type == typeof(DateTime))
                return PrimitiveType.DateTime;
            else if (type.IsArray)
            {
                Type elementType = type.GetElementType()!;
                return MapTypeToPrimitiveType(elementType);
            }
            else
                throw new ArgumentException("Kiểu dữ liệu không hỗ trợ");
        }
        public static string ToStringPrimitiveType(PrimitiveType type) { return type.ToString(); }
        public override void Add(BasicComponent basicComponent)
        {
            // nguyên nhân tạo cái này là tránh việc triển khai nhiều đối tượng
            // và hơn nữa, nếu có một đối tượng nào đó kế thừa thằng này thì cũng không thể thêm
            // phần tử con <Add> vào đối tượng đó được nữa (vì PrimeType chỉ khác Null trong class này)
            Type currentType = basicComponent.GetType();
            if (currentType.IsSubclassOf(currentType) && PrimeType != PrimitiveType.Null)
            {
                if (_children.Count > 0)
                    throw new Exception("Chỉ có một Property/Field được kết nối tới định dạng");
            }
            else
                throw new Exception("Đối tượng lưu trữ kiểu chỉ được thêm thuộc tính liên kết tới " +
                    "Property/Field, phần tử được thêm bắt buộc chỉ được 1 PrimitiveDataComponent");
            base.Add(basicComponent);
            // Kiểm tra class kế thừa thằng này, nếu có thì sẽ có Perent 
            if (basicComponent.GetType().GetProperty("Parent") != null)
                basicComponent.GetType().GetProperty("Parent")!.SetValue(basicComponent, this);
        }
    }

    #region Extension
    public static partial class BasicComponentExtension
    {
        #region Is Inherited Checker 
        public static PrimeComponent? TryGetPrimeComponent(this BasicComponent bcp)
        {
            // nếu bcp có dạng thực tại là EntityComponent thì trả về true 
            bool isEc = bcp.GetType().IsSubclassOf(typeof(PrimeComponent));
            return isEc ? (PrimeComponent)bcp : null;
        }
        #endregion Is Inherited Checker 
        /// <summary>
        /// Nguyên nhân chính là bạn chỉ có một biến PrimitiveType và nó có thể là một Array 
        /// thì bạn không thể lưu được 2 kiểu vào cùng một biến
        /// Cách đơn giản lúc này là cứ lưu kiểu đơn giản vào BsonKey
        /// Còn nếu là Array thì xét thêm BsonValue là Array 
        /// </summary>
        /// <param name="entityComponent"></param>
        /// <param name="primeType"></param>
        public static void AddPrimeComponent(this EntityComponent entityComponent, Type primeType)
        {
            entityComponent.Add(new PrimeComponent(primeType));
        }
    }
    #endregion Extension
}