
namespace LMY.Workflow
{
    public class LMYWFEngineRequest
    {
        public string LMYWFEngineRequestID { get; set; }
        public string WorkFlowName { get; set; }
        public string LMYWFEngineRequestVersionID { get; set; }
        public string Status { get; set; }
        public string RequestCustomData { get; set; }
        public string Created_BY { get; set; }
        public DateTime Created_DATE { get; set; }
        public string Modified_BY { get; set; }
        public DateTime Modified_DATE { get; set; }
    }
}