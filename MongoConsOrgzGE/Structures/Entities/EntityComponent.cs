using MongoConsOrgzGE.Structures.Common;
using MongoDB.Bson;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MongoConsOrgzGE.Structures.Entities
{
    #region Class
    /// <summary>
    /// Tất nhiên nó cũng hoạt động tương tự như BasicComponent 
    /// Nó dùng để phân tách đâu là thực thể
    /// Những lớp đươc khởi tạo đầu thường (BasicComponent) là thành phẩn chủ chốt
    /// Bất kỳ đối tượng nào muốn được tạo ra cũng cân kế thừa lớp này
    /// Ví dụ bạn có một đới tượng DesertEagle, trong này bạn có một Property là Bullet, Bullet có thông số cỡ đạn (Size)
    /// Thì lúc này DerestEagle, Bullet và Size sẽ kế thừa DataObject. 
    /// Và nó sẽ có phân phối tương tự như BasicComponent
    /// Nhưng thay vì dùng cả một tree tổng hợp như trên thì bạn có thể sử dụng một tree riêng biệt cho mỗi đối tượng
    /// Các đối tượng là child của BasicComponent sẽ được quản lý bởi _indexInLocalGraph
    /// Còn các đối tượng là child của DataObject sẽ có một tham số riêng theo cây của nó
    /// Cụ thể là _indexInObjectGraph 
    /// Cách nhanh nhất để tạo một cây với tham số độc lập là Build cây từ trước rồi gắn tham số cho chúng
    /// </summary>
    public class EntityComponent : BasicComponent
    {
        // cái này tương tự như _indexInLocalGraph và cũng chỉ nên khởi tạo khi hoàn thành 
        private int _indexInObjectGraph = default!;
        public int IndexInObjectGraph => _indexInObjectGraph;
        private Dictionary<FString, int> _childrenObjkeys; // cái đầu là từ khóa, cái thứ 2 là vị trí 
        private List<EntityComponent> _childrenObj;
        public List<Tuple<string, int>> ObjectContains()
        {
            return _childrenObjkeys.Select(x => new Tuple<string, int>(x.Key.ToString(), x.Value)).ToList();
        }
        private bool IsHasObjectKey(string key)
        {
            return _childrenObjkeys.Any(k => k.Key.ToString() == key);
        }
        public EntityComponent() : base()
        {
            _childrenObjkeys = new Dictionary<FString, int>();
            _childrenObj = new List<EntityComponent>();
        }
        public EntityComponent(BsonValue bsonObjectKey, BsonValue bsonValueKey)
            : base(bsonObjectKey, bsonValueKey)
        {
            _childrenObjkeys = new Dictionary<FString, int>();
            _childrenObj = new List<EntityComponent>();
        }
        public EntityComponent(string json) : base(json)
        {
            _childrenObjkeys = new Dictionary<FString, int>();
            _childrenObj = new List<EntityComponent>();
        }
        public void ReloadChildrenObjectKeys()
        {
            this.ConvertCurrentIndexGraphIntoObjectGraph(this);
        }
        /// <summary>
        ///  chỉ gọi khi cây đã hoàn tất 
        /// </summary>
        private void ConvertCurrentIndexGraphIntoObjectGraph(EntityComponent dObj, int inLoop = 0)
        {
            if (dObj._childrenkeys != null && dObj._childrenkeys.Count > 0)
            {
                dObj._indexInObjectGraph = inLoop;
                foreach (var child in dObj._children)
                {
                    if (child.TryGetEntityComponent() != null)
                    {
                        dObj._childrenObjkeys.Add(new FString(child.BsonKey.ToString()!), inLoop);
                        ConvertCurrentIndexGraphIntoObjectGraph(child.TryGetEntityComponent()!, inLoop + 1);
                    }
                }
            }
        }
    }
    #endregion Class

    #region Extension
    public static partial class EntityComponentExtension { }
    public static partial class BasicComponentExtension
    {
        #region Is Inherited Checker 
        /// <summary>
        /// Chuyển đổi từ BasicComponent sang EntityComponent nếu đáp ứng được khi bị ép kiểu ngược
        /// </summary>
        /// <param name="bcp"></param>
        /// <returns></returns>
        public static EntityComponent? TryGetEntityComponent(this BasicComponent bcp)
        {
            // nếu bcp có dạng thực tại là EntityComponent thì trả về true 
            bool isEc = bcp.GetType() == typeof(EntityComponent) || bcp.GetType().IsSubclassOf(typeof(EntityComponent));
            return isEc ? (EntityComponent)bcp : null;
        }
        #endregion Is Inherited Checker 
    }
    #endregion Extension

    #region Attribute
    //[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    //public class EntityComponentAttribute : Attribute
    //{
    //    EntityComponent entityComponent;
    //    public EntityComponentAttribute()
    //    {
    //        entityComponent = new EntityComponent();
    //    }
    //    public EntityComponentAttribute(BsonValue bsonObjectKey, BsonValue bsonValueKey)
    //    {
    //        entityComponent = new EntityComponent(bsonObjectKey, bsonValueKey);
    //    }
    //    public EntityComponentAttribute(string json)
    //    {
    //        entityComponent = new EntityComponent(json);
    //    }
    //    public EntityComponent GetEntityComponent()
    //    {
    //        return entityComponent;
    //    }
    //}
    //public class MyClass
    //{
    //    [EntityComponent] // attribute
    //    public string MyProperty { get; set; } = null!;
    //    [EntityComponent]
    //    public int MyField;

    //    public void AccessEntityComponents()
    //    {
    //        // Lấy thông tin lớp của MyClass
    //        Type myClassType = typeof(MyClass);

    //        // Duyệt qua tất cả properties
    //        foreach (var property in myClassType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    //        {
    //            // Kiểm tra xem property có được gắn EntityComponentAttribute không
    //            var attribute = property.GetCustomAttribute<EntityComponentAttribute>();
    //            if (attribute != null)
    //            {
    //                // Truy cập EntityComponent từ attribute
    //                EntityComponent entityComponent = attribute.GetEntityComponent();
    //                //Console.WriteLine($"Property: {property.Name}, EntityComponent: {entityComponent}");
    //            }
    //        }

    //        // Duyệt qua tất cả fields
    //        foreach (var field in myClassType.GetFields(BindingFlags.Public | BindingFlags.Instance))
    //        {
    //            // Kiểm tra xem field có được gắn EntityComponentAttribute không
    //            var attribute = field.GetCustomAttribute<EntityComponentAttribute>();
    //            if (attribute != null)
    //            {
    //                // Truy cập EntityComponent từ attribute
    //                EntityComponent entityComponent = attribute.GetEntityComponent();
    //                //Console.WriteLine($"Field: {field.Name}, EntityComponent: {entityComponent}");
    //            }
    //        }
    //    }
    //}
    #endregion Attribute
}