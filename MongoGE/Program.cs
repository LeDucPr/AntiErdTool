using MongoGE.Connections;
using MongoDB.Bson;
using System.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using ZstdSharp.Unsafe;
using System.Reflection;

namespace MongoGE
{
    public class Uzi
    {
        // viết các thuộc tính của súng tiểu liên Uzi
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public int Damage { get; set; }
        public int FireRate { get; set; }
        public int MagazineSize { get; set; }
        public int ReloadTime { get; set; }
        public int Recoil { get; set; }
    }

    public class Monster
    {
        // viết một số thuộc tính của Monster
        public string Name { get; set; }
        public int Level { get; set; }
        public int Health { get; set; }
        public int Mana { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Speed { get; set; }
        public Type Type { get; set; }
        public Uzi Uzi { get; set; }

        // tạo một phương thức in tất cả thuộc tính của monster bằng GetProperties
        public Dictionary<Type, PropertyInfo[]> PrintProperties()
        {
            Dictionary<Type, PropertyInfo[]> pros = new Dictionary<Type, PropertyInfo[]>();
            pros.Add(typeof(Monster), GetType().GetProperties());
            PropertiesRecursion(this.GetType(), ref pros);
            return pros;
        }
        public void PropertiesRecursion(Type currentType, ref Dictionary<Type, PropertyInfo[]> pros)
        {
            var properties = currentType.GetProperties();
            foreach (var property in properties)
            {
                PropertiesRecursion(property.PropertyType, ref pros);
                System.Console.WriteLine($"{property.Name}: {property.GetValue(this)}");
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;

            // tạo một monster
            Monster monster = new Monster
            {
                Name = "Goblin",
                Level = 1,
                Health = 100,
                Mana = 50,
                Attack = 10,
                Defense = 5,
                Speed = 5
            };
            monster.PrintProperties();


            //Task.Run(async () => await PerformFindBsons()).Wait();
        }
        static async Task PerformFindBsons()
        {
            MongoClientController clientController = new MongoClientController("mongodb://localhost:27017").AddDatabaseControllers(new List<string> { "GeManagerTest" });
            MongoDatabaseController databaseController = clientController["GeManagerTest"].AddCollectionControllers(new List<string> { "ViewMapper" });
            MongoCollectionController collectionController = databaseController["ViewMapper"];
            var bsonView = new BsonDocument(){
                {
                    "View", new BsonDocument(){
                        {
                            "Languages", new BsonDocument(){ // (Collection) View trong (Database) GetmanagerTest được truy cập đầu tiên để tạo ngôn ngữ hiển thị 
                                {
                                    "Vietnamese", new BsonDocument() { // ngôn ngữ mặc định là tiếng Việt 
                                        {
                                            "Phone", new BsonDocument() { // (Collection) Phone trong Collection này chứa các Field định danh tiếng Việt và được map với các Field được đánh mã định danh 
                                                { "Tên điện thoại", "_0" }, // Việc đánh số được thực hiện từ _0, giá trị nào đó là đối tượng không phải là Field thuần thì _ đánh dấu số thứ tự trong Field của nó 
                                                { "Mã Series", "_1" },
                                                {
                                                    "Kích thước", new BsonDocument(){
                                                        { "_", "_2" },
                                                        { "Chiều rộng", "_0" },
                                                        { "Chiều dài", "_1" }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                {
                                    "English", new BsonDocument() { // ngôn ngữ mặc định là tiếng Anh
                                        {
                                            "Phone", new BsonDocument() {
                                                { "Phone Name", "_0" },
                                                { "Series Code", "_1" },
                                                {
                                                    "Size", new BsonDocument(){
                                                        { "_", "_2" },
                                                        { "Width", "_0" },
                                                        { "Height", "_1" }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            //await collectionController.CreateBson(bsonView);
            await collectionController.UpdateSizeValueAsync("Languages.Vietnamese.Phone.Kích thước._", "_2");


            //MongoClientController clientController = new MongoClientController("mongodb://localhost:27017").AddDatabaseControllers(new List<string> { "Client" });
            //MongoDatabaseController databaseController = clientController["Client"].AddCollectionControllers(new List<string> { "Client" });
            //MongoCollectionController collectionController = databaseController["Client"];
            //var findComponents = new Dictionary<string, string>
            //{
            //    { "Id", "00001"}
            //};
            //var documents = await collectionController.FindBsons(findComponents);
            //foreach (var doc in documents)
            //{
            //    Console.WriteLine(doc.ToString());
            //}
        }
    }
}
