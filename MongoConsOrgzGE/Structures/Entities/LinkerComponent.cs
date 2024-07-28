using MongoConsOrgzGE.Structures.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace MongoConsOrgzGE.Structures.Entities
{
    /// <summary>
    ///  Đối tượng liên kết cuối cùng trogn EntityComponent
    ///  Lưu trữ liên kết đến các thành phần của Collection khác
    ///  Và chắc chắn là thành phần cuối thì nó không thể Add được thành phần nào nữa 
    /// </summary>
    public class LinkerComponent : EntityComponent
    {
        private bool _isEnabledReloadValueBson = true;
        public bool IsEnabledReloadValueBson { get => _isEnabledReloadValueBson; }
        /// <summary>
        /// Key lưu giá trị link tới và là đối số trong đối tượng tham chiếu 
        /// Value lưu đường dẫn tuyệt đối của Collection tham chiếu tới
        /// </summary>
        /// <param name="key">Tên biến cụ thể tại Collection cần khai thác link tới</param>
        /// <param name="value">Mặc định là rỗng để khai thác đường dẫn tuyệt đối</param>
        /// <param name="isEnabledReload">Mặc định đặt bằng true để có thể ghi đè đường dẫn tuyệt đối</param>
        public LinkerComponent(BsonValue key, string value = null!, bool isEnabledReloadValueBson = true)
            : base(key, value ?? string.Empty)
        {
            _isEnabledReloadValueBson = isEnabledReloadValueBson;
        }
        public override void Add(BasicComponent? basicComponent)
        {
            throw new System.Exception("Không thể thêm thành phần vào LinkerComponent");
        }
    }
    #region Extension
    public static partial class BasicComponentExtension
    {
        /// <summary>
        /// Cắm đường dẫn tuyệt đối vào trong Value 
        /// </summary>
        /// <param name="ecp"></param>
        /// <exception cref="Exception"></exception>
        public static void CreateAbsolutizationPathsValue(this BasicComponent ecp, List<string> paths = null!)
        {
            paths = paths ?? new List<string>() { ecp.BsonKey.ToString()! };
            foreach (var ecpChild in ecp.Children)
            {
                paths.Add(ecpChild.BsonKey.ToString()!);
                if (ecpChild.GetType().IsSubclassOf(typeof(EntityComponent)))
                {
                    if (!ecpChild.TryGetLinkerComponent()!.IsEnabledReloadValueBson)
                        throw new Exception("Không thay đổi được BsonValue khi không cho phép thay đổi");
                    ecpChild.BsonValue = string.Join(".", paths);
                }
                else
                    CreateAbsolutizationPathsValue(ecpChild, paths);
                paths.RemoveAt(paths.Count - 1);
            }
        }
        #region Is Inherited Checker 
        /// <summary>
        /// Chuyển đổi từ BasicComponent sang EntityComponent nếu đáp ứng được khi bị ép kiểu ngược
        /// </summary>
        /// <param name="bcp"></param>
        /// <returns></returns>
        public static LinkerComponent? TryGetLinkerComponent(this BasicComponent bcp)
        {
            // nếu bcp có dạng thực tại là EntityComponent thì trả về true 
            bool isEc = bcp.GetType() == typeof(LinkerComponent) || bcp.GetType().IsSubclassOf(typeof(LinkerComponent));
            return isEc ? (LinkerComponent)bcp : null;
        }
        #endregion Is Inherited Checker 
    }
    #endregion Extension
}
