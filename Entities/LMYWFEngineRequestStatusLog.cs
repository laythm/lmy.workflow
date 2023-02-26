namespace LMY.Workflow
{
    public class LMYWFEngineRequestStatusLog
    {
        public string LMYWFEngineLogID { get; set; }
        public string LMYWFEngineRequestID { get; set; }
        public string WorkFlowName { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public string Created_BY { get; set; }
        public DateTime Created_DATE { get; set; }
        public string Modified_BY { get; set; }
        public DateTime Modified_DATE { get; set; }
    }
}