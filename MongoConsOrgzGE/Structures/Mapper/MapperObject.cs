using MongoConsOrgzGE.Structures.Common;
using MongoConsOrgzGE.Structures.Common.Enums;
using MongoConsOrgzGE.Structures.Views;
using MongoDB.Driver.Core.Operations;

namespace MongoConsOrgzGE.Structures.Mapper
{
    /// <summary>
    /// Class thực thi việc kết nối Data thoogn qua định danh trong hệ thống
    /// Class này bao gồm tất cả các việc thực thi có trong bộ kết nối tới đối tượng
    /// và điều khiên đối tượng thông qua thành phần cung chung gian 
    /// Cây(tree) dữ liệu định danh được xử lý trực tiếp tại đây 
    /// </summary>
    public class MapperObject
    {
        private List<OverallUV> _overallUVs;
        public void Add(OverallUV overallUV)
        {
            if (overallUV == null || overallUV == default)
                throw new ArgumentNullException(nameof(overallUV), "OverallUV không được phép null hoặc phải khởi tạo bộ chức năng");
            _overallUVs.Add(overallUV);
        }
        public MapperObject(bool isCreateDefaut = true)
        {
            _overallUVs = new List<OverallUV>();
            if (isCreateDefaut)
                this.CreateDefault();
        }
        private void CreateDefault()
        {
            _overallUVs.Add(new View());
            this[EMapperType.View].InstanceCollectionController();

            //_overallUVs.Add(new Unit());
            //_overallUVs.Add(new UVConnection());
        }
        public OverallUV this[EMapperType eMapperType]
        {
            get
            {
                switch (eMapperType)
                {
                    case EMapperType.View:
                        return _overallUVs.FirstOrDefault(x => x is View) ?? throw new Exception($"{View.DocumentName} không tồn tại");
                    //case EMapperType.Unit:
                    //    return _mapperUV;
                    //case EMapperType.UVConnection:
                    //    return _mapperUV;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eMapperType), eMapperType, "EMapperType không được hỗ trợ");
                }
            }
        }
        /// <summary>
        /// Các bộ khởi tạo mặc định
        /// </summary>
        /// <param name="eMapperType"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Add(EMapperType eMapperType)
        {
            switch (eMapperType)
            {
                case EMapperType.View:
                    _overallUVs.Add(new Views.View());
                    break;
                //case EMapperType.Unit:
                //    _mapperUV = new Unit();
                //    break;
                //case EMapperType.UVConnection:
                //    _mapperUV = new UVConnection();
                //    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eMapperType), eMapperType, "EMapperType không được hỗ trợ");
            }
        }
    }
}
