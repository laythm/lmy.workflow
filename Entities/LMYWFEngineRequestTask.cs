using System.Text.Json;

namespace LMY.Workflow
{
    public class LMYWFEngineRequestTask
    {
        public string LMYWFEngineTaskID { get; set; }
        public string WorkFlowName { get; set; }
        public string LMYWFEngineRequestID { get; set; }
        public string UserGroup { get; set; }
        public string DelegatedToUser { get; set; }
        public string Created_BY { get; set; }
        public DateTime Created_DATE { get; set; }
        public string Modified_BY { get; set; }
        public DateTime Modified_DATE { get; set; }

        public LMYWFEngineRequest Request { get; set; }


        //public string TaskName { get { return AvailableTransitions == null ? null : string.Join(", ", AvailableTransitions.Select(x => x.TransitionName).ToArray()); } }
        public LMYWFEngineTransition[] AvailableTransitions { get; set; }
    }
}