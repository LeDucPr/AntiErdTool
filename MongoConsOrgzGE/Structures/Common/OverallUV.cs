using MongoDB.Bson;
using MongoGE.Connections;
using MongoConsOrgzGE;
using MongoDB.Bson.IO;
using MongoConsOrgzGE.Structures.Entities;

namespace MongoConsOrgzGE.Structures.Common
{
    /// <summary>
    /// Kiến trúc được sinh ra cho phép các đối tượng View, Unit, UVConnetion kế thừa
    /// </summary>
    public class OverallUV
    {
        #region CreateType
        enum CreateType
        {
            Default,
            JsonKeyValue,
            KeyNameAndJsonValue,
            KeyNameAndBasicComponent
        }
        private CreateType _createType = CreateType.Default;
        #endregion CreateType
        /// <summary>
        /// Tên của thằng được lấy làm khóa của cây 
        /// </summary>
        protected virtual string Name { get; set; } = default!; // mặc định không đặt tên 
        private BasicComponent _basicComponent = default!;
        /// <summary>
        /// Đây sẽ là một cây tổng thể lưu trữ các thành phần được nối kiểu cây dữ liệu json
        /// </summary>
        public BasicComponent BasicComponent { get => this._basicComponent; }
        /// <summary>
        /// Document được liên kết 
        /// </summary>
        protected MongoCollectionController _mongoCollectionController = null!;
        public virtual void InstanceCollectionController()
        {
            throw new Exception("Yêu cầu ghi đè vào lớp kế thừa");
        }
        #region Bộ ghi mặc định
        private void CreateConstructorInMongoDB()
        {
            // bước này thực sự không cần thiết nếu đã được thực thi từ trước, ghi này cho chắc cú 
            Setup.InitializeMongoClient();
        }
        protected virtual BasicComponent DefaultConstructor()
        {
            BasicComponent _0view = new BasicComponent(Name, new BasicComponent()); // value -> langueges
            BasicComponent _1languages = new BasicComponent("Languages", new BsonDocument());
            // Vietnamese
            BasicComponent _2vietnamese = new BasicComponent("Vietnamese", new BsonDocument());
            // dưới đây là cấu trúc bảng, có ghì muốn tạo thì sau ghi đè vào 

            BasicComponent _3Phone = new EntityComponent("Phone", new BsonDocument());
            BasicComponent _4PhoneName = new EntityComponent("Tên điện thoại", new BsonDocument());
            BasicComponent _4SeriesCode = new EntityComponent("Mã Series", new BsonDocument());
            BasicComponent _4Size = new EntityComponent("Kích thước", new BsonDocument());
            BasicComponent _4Color = new EntityComponent("Màu sắc", new BsonDocument());

            BasicComponent _5Size = new EntityComponent("_", new BsonDocument());
            BasicComponent _5Width = new EntityComponent("Chiều rộng", new BsonDocument());
            BasicComponent _5Height = new EntityComponent("Chiều dài", new BsonDocument());

            // Linker
            BasicComponent _4PhoneNameLinker = new LinkerComponent("_0", string.Empty);
            BasicComponent _4SeriesCodeLinker = new LinkerComponent("_1", string.Empty);
            BasicComponent _4ColorLinker = new LinkerComponent("_3", string.Empty);
            // Do _4Size là EntityComponent nên thông qua một đối tượng dưới cấp để mô tả 
            BasicComponent _5SizeLinker = new LinkerComponent("_2", string.Empty);
            BasicComponent _5WidthLinker = new LinkerComponent("_0", string.Empty);
            BasicComponent _5HeightLinker = new LinkerComponent("_1", string.Empty);

            _0view.Add(_1languages);
            _1languages.Add(_2vietnamese);
            // link 
            _2vietnamese.Add(_3Phone);
            _3Phone.Add(_4PhoneName);
            _3Phone.Add(_4SeriesCode);
            _3Phone.Add(_4Size);
            _3Phone.Add(_4Color);
            _4Size.Add(_5Size);
            _4Size.Add(_5Width);
            _4Size.Add(_5Height);
            // Linker
            _4PhoneName.Add(_4PhoneNameLinker);
            _4SeriesCode.Add(_4SeriesCodeLinker);
            _4Color.Add(_4ColorLinker);
            _5Size.Add(_5SizeLinker);
            _5Width.Add(_5WidthLinker);
            _5Height.Add(_5HeightLinker);

            _0view.ReloadChildrenKeys(); // khởi tạo bộ _key 
            _3Phone.TryGetEntityComponent()!.ReloadChildrenObjectKeys(); // khởi tạo bộ _key
            _3Phone.TryGetEntityComponent()!.CreateAbsolutizationPathsValue(); // đường dẫn tuyệt đối cho LinkerComponent 
            // Kết nối DB và khởi tạo giá trị cho bộ kết nối duy nhất 
            
            _0view.ReloadChildrenKeys(); // khởi tạo bộ _key 
            CreateConstructorInMongoDB();
            return _0view;
        }
        #endregion Bộ ghi mặc định

        #region Overload Constructors
        public OverallUV()
        {
            _createType = CreateType.Default;
            _basicComponent = DefaultConstructor(); // lúc này Name được ghi đè trong lớp kế thừa 
        }
        public OverallUV(string keyName, BasicComponent basicComponent)
        {
            _createType = CreateType.KeyNameAndBasicComponent;
            Name = keyName;
            _basicComponent = basicComponent;
            _basicComponent.BsonKey = keyName;
        }
        public OverallUV(string keyName, string jsonValue)
        {
            _createType = CreateType.KeyNameAndJsonValue;
            Name = keyName; // BsonKey.ToString()
            _basicComponent.BsonKey = keyName;
            _basicComponent = BasicComponent.CreateBasicComponentFromJson(jsonValue);
        }
        public OverallUV(string jsonKeyValue)
        {
            _createType = CreateType.JsonKeyValue;
            _basicComponent = BasicComponent.CreateBasicComponentFromJson(jsonKeyValue);
            Name = _basicComponent.BsonKey.ToString()!;
        }
        public void UpdateDocumentEntity(List<string> keyPaths, BsonValue value)
        {
            try
            {
                BasicComponent bcp = _basicComponent;
                if (bcp.FindChild(keyPaths).TryGetEntityComponent() is EntityComponent entityComponent)
                {
                    keyPaths.Insert(0, entityComponent.BsonKey.ToString()!);
                    string concatString = string.Join(".", keyPaths);
                    entityComponent.ReloadChildrenObjectKeys();
                    _mongoCollectionController.UpdateSizeValueAsync(concatString, value.ToString()!).Wait();
                }
            }
            catch { throw new Exception("Không thể cập nhật vì không phải là EntityComponent"); }
        }
        #endregion Overload Constructors
        #region Ví dụ
        //BasicComponent _0view = new BasicComponent("View", new BasicComponent()); // value -> langueges
        //BasicComponent _1languages = new BasicComponent("Languages", new BsonDocument());
        //// Vietnamese
        //BasicComponent _2vietnamese = new BasicComponent("Vietnamese", new BsonDocument());
        //BasicComponent _3phoneVietnamese = new BasicComponent("Phone", new BsonDocument());
        //BasicComponent _4phoneNameVietnamese = new BasicComponent("Tên điện thoại", "_0");
        //BasicComponent _4seriesCodeVietnamese = new BasicComponent("Mã Series", "_1");
        //BasicComponent _4sizeVietnamese = new BasicComponent("Kích thước", new BsonDocument());
        //BasicComponent _5size_Vietnamese = new BasicComponent("_", "_2");
        //BasicComponent _5widthVietnamese = new BasicComponent("Chiều rộng", "_0");
        //BasicComponent _5heightVietnamese = new BasicComponent("Chiều dài", "_1");
        ////English
        //BasicComponent _2english = new BasicComponent("English", new BsonDocument());
        //BasicComponent _3phoneEnglish = new BasicComponent("Phone", new BsonDocument());
        //BasicComponent _4phoneNameEnglish = new BasicComponent("Phone Name", "_0");
        //BasicComponent _4seriesCodeEnglish = new BasicComponent("Series Code", "_1");
        //BasicComponent _4sizeEnglish = new BasicComponent("Size", new BsonDocument());
        //BasicComponent _5size_English = new BasicComponent("_", "_2");
        //BasicComponent _5widthEnglish = new BasicComponent("Width", "_0");
        //BasicComponent _5heightEnglish = new BasicComponent("Height", "_1");

        //_0view.Add(_1languages);
        //_1languages.Add(_2vietnamese);
        //// Vietnamese
        //_2vietnamese.Add(_3phoneVietnamese);
        //_3phoneVietnamese.Add(_4phoneNameVietnamese);
        //_3phoneVietnamese.Add(_4seriesCodeVietnamese);
        //_3phoneVietnamese.Add(_4sizeVietnamese);
        //_4sizeVietnamese.Add(_5size_Vietnamese);
        //_4sizeVietnamese.Add(_5widthVietnamese);
        //_4sizeVietnamese.Add(_5heightVietnamese);
        //// English
        //_1languages.Add(_2english);
        //_2english.Add(_3phoneEnglish);
        //_3phoneEnglish.Add(_4phoneNameEnglish);
        //_3phoneEnglish.Add(_4seriesCodeEnglish);
        //_3phoneEnglish.Add(_4sizeEnglish);
        //_4sizeEnglish.Add(_5size_English);
        //_4sizeEnglish.Add(_5widthEnglish);
        //_4sizeEnglish.Add(_5heightEnglish);

        //_0view.ReloadChildrenKeys();
        ////Console.WriteLine(_0view); 

        ////// in ra màn hình _0view
        ////System.Console.WriteLine(_0view.BsonValue.ToString());
        ////_1languages.Contains().ForEach(x => System.Console.WriteLine($"{x.Item1}, {x.Item2}"));

        ////Console.WriteLine();
        ////Console.WriteLine();


        ////var a = _0view.FindAll(2);
        ////foreach (var item in a)
        ////{
        ////    foreach (var b in item)
        ////    {
        ////        Console.WriteLine(b);
        ////    }
        ////    Console.WriteLine();
        ////}

        ////Console.WriteLine();

        ////Console.WriteLine(_1languages["Vietnamese"]?.BsonValue); 
        ////_0view.FindChild(a.First()!).Contains().ForEach(x => Console.WriteLine($"{x.Item1}, {x.Item2}"));
        ////_0view.FindChild(a.Last()!).Contains().ForEach(x => Console.WriteLine($"{x.Item1}, {x.Item2}"));

        //string json = _3phoneEnglish.ToString()!;
        //var a = BasicComponent.CreateBasicComponentFromJson(json);
        //Console.WriteLine(a);
        #endregion Ví dụ
    }
}
