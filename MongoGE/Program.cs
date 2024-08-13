using MongoDB.Bson;
using MongoGE.Connections;
using System.Text.RegularExpressions;

namespace MongoGE
{

    internal class Program
    {
        static void Main(string[] args)
        {
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;


        }

        public class A
        {
            public static Regex CreateEmailRegex()
            {
                var regexConfig = new AggregationsConf.RegexConfig();
                // Bật ký tự bắt đầu và kết thúc
                regexConfig.EnableStartWithCaret(true);
                regexConfig.EnableEndWithDollar(true);
                // Tên email: ký tự viết hoa, viết thường, số
                regexConfig.AddPattern(@"[a-zA-Z0-9]+");
                // Ký tự "@"
                regexConfig.AddPattern(@"@");
                // Mã miền là "gmail"
                regexConfig.AddPattern(@"gmail");
                // Vùng gì đó (có thể là bất kỳ ký tự nào)
                regexConfig.AddPattern(@"\.[a-zA-Z]+");
                // Kết thúc với ".com"
                regexConfig.AddPattern(@"\.com");
                // bật không phân biệt chữ hoa chữ thường
                regexConfig.EnableIgnoreCase(true);
                Console.WriteLine("Generated Email Regex string: " + regexConfig.ToString());
                // Tạo và trả về regex
                return regexConfig.CreateRegex();
            }

            public static void TestRegex()
            {
                Regex emailRegex = CreateEmailRegex();
                Console.WriteLine("Generated Email Regex: " + emailRegex);

                // Test the regex
                List<string> testEmails = new List<string>
                {
                    "testA123@gmail.fwe.com",
                    "Test.Email@gmail.com",
                    "invalid-email@gmail",
                    "another.test@notgmail.com"
                };

                foreach (var email in testEmails)
                {
                    Console.WriteLine($"{email} is valid: {emailRegex.IsMatch(email)}");
                }
            }
        }

        static void TestAggregation()
        {
            MongoClientController clientController = new MongoClientController("mongodb://localhost:27017").AddDatabaseControllers(new List<string> { "Book" });
            MongoDatabaseController databaseController = clientController["Book"].AddCollectionControllers(new List<string> { "Book", "Client" });
            MongoCollectionController ColCtrl_Book = databaseController["Book"];
            MongoCollectionController ColCtrl_Client = databaseController["Client"];


            //// Xây dựng pipeline cho aggregation
            //var pipeline = new List<BsonDocument>
            //{
            //    new BsonDocument("$lookup", new BsonDocument
            //    {
            //        { "from", "Client" },
            //        { "localField", "Id" },
            //        { "foreignField", "ReadedId._v" },
            //        { "as", "Clients" }
            //    }),
            //    new BsonDocument("$unwind", "$Clients"),
            //    new BsonDocument("$project", new BsonDocument
            //    {
            //        { "_id", 0 },
            //        { "BookData", "$$ROOT" },
            //        { "ClientData", "$Clients" }
            //    })
            //};
            //List<BsonDocument> result = ColCtrl_Book.AggregateBsons(pipeline).GetAwaiter().GetResult(); // AggregateBsons
            //foreach (var doc in result) { Console.WriteLine(doc.ToJson()); }


            var pipeline = new List<BsonDocument>
            {
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Book" },
                    { "let", new BsonDocument("readedIds", "$ReadedId._v") },
                    { "pipeline", new BsonArray
                        {
                            new BsonDocument("$match", new BsonDocument
                            {
                                { "$expr", new BsonDocument
                                    {
                                        { "$in", new BsonArray { "$Id", "$$readedIds" } }
                                    }
                                }
                            })
                        }
                    },
                    { "as", "Books" }
                }),
                new BsonDocument("$unwind", "$Books"),
                new BsonDocument("$project", new BsonDocument
                {
                    { "_id", 0 },
                    { "Book", "$Books" },
                    { "Client", new BsonDocument
                        {
                            { "_id", "$_id" },
                            { "name", "$name" },
                            { "email", "$email" }
                        }
                    }
                }),
                new BsonDocument("$facet", new BsonDocument
                {
                    { "books", new BsonArray
                        {
                            new BsonDocument("$match", new BsonDocument
                            {
                                { "Book", new BsonDocument { { "$ne", BsonNull.Value } } }
                            })
                        }
                    },
                    { "client", new BsonArray
                        {
                            new BsonDocument("$limit", 1),
                            new BsonDocument("$project", new BsonDocument
                            {
                                { "_id", 0 },
                                { "Book", BsonNull.Value },
                                { "Client", new BsonDocument
                                    {
                                        { "_id", "$_id" },
                                        { "name", "$name" },
                                        { "email", "$email" }
                                    }
                                }
                            })
                        }
                    }
                }),
                new BsonDocument("$project", new BsonDocument
                {
                    { "combined", new BsonDocument
                        {
                            { "$concatArrays", new BsonArray { "$client", "$books" } }
                        }
                    }
                }),
                new BsonDocument("$unwind", "$combined"),
                new BsonDocument("$replaceRoot", new BsonDocument("newRoot", "$combined"))
            };
            List<BsonDocument> result = ColCtrl_Client.AggregateBsons(pipeline).GetAwaiter().GetResult();
            foreach (var doc in result) { Console.WriteLine(doc.ToJson()); }
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
