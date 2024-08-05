using MongoConsOrgzGE.Structures.Entities;
using MongoDB.Bson.IO;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace MongoConsOrgzGE.Structures.Common
{
    /// <summary>
    /// Map trực tiếp đường dẫn của BasicComponent sang đối tượng cụ thể ghi dữ liệu trên MongoDB
    /// Nói đùng hơn là PipelineConnector sẽ chứa tất cả các thành phần map lại 
    /// tương ứng với basicComponent được tryền vào 
    /// </summary>
    public class PipelineConnector
    {
        private List<(string ConcatStringConnection, string CollectionName, bool IsActive)> _pipelines = null!;
        private BasicComponent _basicComponent = null!; // tham chiếu Component được truyền vào
        public PipelineConnector()
        {
            _pipelines = new List<(string, string, bool)>();
        }
        /// <summary>
        /// Hãy đảm bảo rằng đây là bcp tổng thể, không phải là bcp con
        /// Tự động tạo tất cả các kết nối tới thông só tự tạo đầu tiên là EntityComponent
        /// </summary>
        /// <param name="bcp"></param>
        /// <param name="indexStartEntity"></param>
        /// <exception cref="Exception"></exception>
        public PipelineConnector(BasicComponent bcp)
        {
            _basicComponent = bcp;
            _pipelines = new List<(string, string, bool)>();
            try
            {
                List<EntityComponent> ecpStarters = new List<EntityComponent>();
                this.FindStartEntityIndexs(bcp, ref ecpStarters);
                foreach (EntityComponent ecpStarter in ecpStarters)
                {
                    List<string> paths = new List<string>();
                    this.CreateAllAbsolutizationcomponentOfPathsWithAllComponents(ecpStarter, ref paths);
                    // mặc định là không mở kết nối (tức là không cho phép thay đổi)
                    _pipelines.AddRange(paths.Select(p => (p, ecpStarter.BsonKey.ToString()!, false)));                    
                }
                _pipelines.ForEach(p => System.Console.WriteLine(p.CollectionName + "   " + p.ConcatStringConnection));
            }
            catch (Exception ex)
            {
                throw new Exception("Không thể tạo PipelineConnector từ BasicComponent", ex);
            }
        }
        private void FindStartEntityIndexs(BasicComponent bcp, ref List<EntityComponent> sIdx)
        {
            sIdx = sIdx ?? new List<EntityComponent>();
            foreach (BasicComponent childBcp in bcp.Children)
            {
                if (childBcp is EntityComponent c) { sIdx.Add(c); continue; }
                FindStartEntityIndexs(childBcp, ref sIdx);
            }
        }
        private void CreateAllAbsolutizationcomponentOfPathsWithAllComponents(EntityComponent ecp, ref List<string> paths, List<string> componentOfPaths=null!)
        {
            componentOfPaths = componentOfPaths ?? new List<string>() { ecp.BsonKey.ToString()! };

            if (ecp.GetType().IsSubclassOf(typeof(EntityComponent)))
            {
                paths = paths ?? new List<string>();
                paths.Add(string.Join(".", componentOfPaths));
                return;
            }
            foreach (EntityComponent ecpChild in ecp.Children)
            {
                componentOfPaths.Add(ecpChild.BsonKey.ToString()!);
                CreateAllAbsolutizationcomponentOfPathsWithAllComponents(ecpChild, ref paths, componentOfPaths);
                componentOfPaths.RemoveAt(componentOfPaths.Count - 1);
            }
        }
    }

    public static class PipelineConnectorExtension { }
}
