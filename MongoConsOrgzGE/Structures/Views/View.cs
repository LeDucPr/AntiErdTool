using MongoConsOrgzGE.Structures.Common;
using MongoConsOrgzGE;

namespace MongoConsOrgzGE.Structures.Views
{
    public class View : OverallUV
    {
        private static readonly string _name = "View";
        public static string DocumentName => _name;
        protected override string Name
        {
            get { return _name; } // Hoặc trả về một giá trị khác tùy thuộc vào logic của bạn
        }
        public View() : base() { }//{BasicComponent.KeyConcat("Vietnamese", 2).ForEach(x => x.ForEach(x=> Console.WriteLine(x)));}
        public View(string jsonKeyValue) : base(jsonKeyValue) { }
        public View(string keyName, string jsonValue) : base(keyName, jsonValue) { }
        public View(string keyName, BasicComponent basicComponent) : base(keyName, basicComponent) { }
        public override void InstanceCollectionController()
        {
            try { Setup.AddDocumentToMasterCollection(this.BasicComponent); }
            catch (AggregateException) { } // lỗi này xuất hiện khi document đã tồn tại (View)
            finally
            {
                Setup.GetMasterCollection().Indexes((this.BasicComponent.BsonKey.ToString(), true)!).Wait(); // đây là Task nên cần Wait()
            }
        }
    }
}