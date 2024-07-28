using System.Net.WebSockets;
using System.Runtime.CompilerServices;

namespace MongoConsOrgzGE.Structures.Common
{
    /// <summary>
    /// Map trực tiếp đường dẫn của BasicComponent sang đối tượng cụ thể ghi dữ liệu trên MongoDB
    /// </summary>
    public class PipelineConnector
    {
        private List<List<string>> pipelines = null!;
        public PipelineConnector()
        {
            pipelines = new List<List<string>>();
        }
        public PipelineConnector(List<List<string>> pipelines)
        {
            this.pipelines = pipelines;
        }
        public void Add(List<string> pipeline)
        {
            pipelines.Add(pipeline);
        }
        public void Add(List<List<string>> newPipelines)
        {
            this.pipelines.AddRange(newPipelines);
        }
    }

    public static class PipelineConnectorExtension
    {
        public static void CreateAll(this BasicComponent bcp, ref PipelineConnector pc)
        {
            try
            {
                int i = 0; 
                while (i < bcp.Children.Count)
                    pc.Add(bcp.FindAll(i));
            }
            catch (Exception ex)
            {
                throw new Exception("Không thể tạo PipelineConnector từ BasicComponent", ex);
            }
            finally
            {
                
            }
        }
    }
}
