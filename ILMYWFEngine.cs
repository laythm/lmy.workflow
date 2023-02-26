using LMY.Workflow.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMY.Workflow
{
    public interface ILMYWFEngine
    {
        internal void Configure(string configFilePath, string dbConnectionString);
        public LMYWFEngineResult<string> StartNew(string workFlowName, string currentUser, string requestCustomData = null);
        public LMYWFEngineResult<LMYWFEngineTransition[]> GetAvailableTransitions(string workFlowName, string LMYWFEngineRequestID, string currentUser, string[] currentUserGroups);
        public LMYWFEngineResult<bool> MoveNext(string workFlowName, string LMYWFEngineRequestID, string LMYWFEngineTransitionID,
                string LMYWFEngineRequestVersionID, string currentUser, string[] currentUserGroups, bool updateRequestCustomData = false, string requestCustomData = null);

        public LMYWFEngineResult<LMYWFEngineRequestTask[]> GetTasks(string workFlowName, string currentUser, string[] currentUserGroups, int page, int pageSize, out int totalRecords);

        public LMYWFEngineResult<LMYWFEngineRequestStatusLog[]> GetRequestLogs(string workFlowName, string LMYWFEngineRequestID);

        public LMYWFEngineResult<bool> DelegateTask(string workFlowName, string LMYWFEngineTaskID, string delegatedToUser, string currentUser);
    }
}
