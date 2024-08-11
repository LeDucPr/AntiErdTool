using MongoConsOrgzGE.Structures.Common;
using MongoConsOrgzGE.Structures.Common.Enums;
using MongoConsOrgzGE.Structures.Entities;
using MongoConsOrgzGE.Structures.Mapper;
using MongoConsOrgzGE.Structures.Views;

namespace MongoConsOrgzGE
{
    internal class Program
    {
        static void Main(string[] args)
        {
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;
            Setup.InitializeMasterCollection();
            MapperObject mapperObject = new MapperObject();

            View view = (View)mapperObject[EMapperType.View];
            Console.WriteLine(view.BasicComponent);
            //Console.WriteLine(view.BasicComponent["Languages"]);
            //Console.WriteLine(view.BasicComponent["Languages"]!["Vietnamese"]!["Phone"]!["Tên điện thoại"]!.TryGetEntityComponent()!.IndexInObjectGraph);
            //Console.WriteLine(view.BasicComponent["Languages"]!["Vietnamese"]!.TryGetEntityComponent()!.IndexInObjectGraph);
            PipelineConnector pipelineConnector = new PipelineConnector(view.BasicComponent);
        }
    }
}
