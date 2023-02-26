using LMY.Workflow.Entities;
using System;
using System.Data;
using System.Text.Json;

namespace LMY.Workflow
{
    public class LMYWFEngine : ILMYWFEngine
    {
        LMYWFEngineConfig _config = new LMYWFEngineConfig();
        ILMYWFEngineDBWrapper _db;
        bool isConfigured = false;

        public LMYWFEngine(ILMYWFEngineDBWrapper db)
        {
            _db = db;
        }

        void ILMYWFEngine.Configure(string configFilePath, string dbConnectionString)
        {
            try
            {
                isConfigured = false;

                string jsonString = File.ReadAllText(configFilePath);

                _config = JsonSerializer.Deserialize<LMYWFEngineConfig>(jsonString);
                _config.DBConnectionString = dbConnectionString;

                for (int i = 0; i < _config.WorkFlows.Count(); i++)
                {
                    if (string.IsNullOrEmpty(_config.WorkFlows[i].WorkFlowName))
                    {
                        throw new Exception("workflow name must be provided");
                    }

                    for (int j = i + 1; j < _config.WorkFlows.Count(); j++)
                    {
                        if (_config.WorkFlows[i].WorkFlowName == _config.WorkFlows[j].WorkFlowName)
                        {
                            throw new Exception("WorkFlow name must be unique");
                        }
                    }

                    foreach (var transition in _config.WorkFlows[i].Transitions)
                    {
                        transition.LMYWFEngineTransitionID = Guid.NewGuid().ToString();

                        if (string.IsNullOrEmpty(transition.NextStatus))
                        {
                            throw new Exception("Transition NextStatus must be provided");
                        }
                    }
                }

                //create tables
          
                _db.ConfigureDB(_config.DBConnectionString);
                _config.Lock();

                isConfigured = true;
            }
            catch (Exception ex)
            {
                if (_db != null)
                {
                    _db.CloseConnection();
                }

                throw new Exception(ErrorCodes.E001, ex);
            }
        }

        public LMYWFEngineResult<string> StartNew(string workFlowName, string currentUser, string requestCustomData = null)
        {
            LMYWFEngineResult<string> lmy_wf_engine_result = new LMYWFEngineResult<string>();

            CheckIsConfigured();
            CheckWorkFlowName(workFlowName);

            try
            {
                _db.BeginTransaction();

                string LMYWFEngineRequestID = Guid.NewGuid().ToString();
                LMYWFEngineDictionary insertData = new LMYWFEngineDictionary()
                {
                    Pairs = new LMYWFEngineKeyValuePair[] {
                        new LMYWFEngineKeyValuePair("LMYWFEngineRequestID", LMYWFEngineRequestID),
                        new LMYWFEngineKeyValuePair("WorkFlowName", workFlowName),
                        new LMYWFEngineKeyValuePair("LMYWFEngineRequestVersionID", Guid.NewGuid().ToString()),
                        new LMYWFEngineKeyValuePair("Status", null),
                        new LMYWFEngineKeyValuePair("RequestCustomData", requestCustomData),
                        new LMYWFEngineKeyValuePair("Created_BY", currentUser),
                        new LMYWFEngineKeyValuePair("Created_DATE", DateTime.Now),
                        new LMYWFEngineKeyValuePair("Modified_BY", currentUser),
                        new LMYWFEngineKeyValuePair("Modified_DATE", DateTime.Now)
                    }
                };

                _db.Insert("LMYWFEngineRequests", insertData);

                _db.CommitTransaction();

                lmy_wf_engine_result.MethodResult = LMYWFEngineRequestID;
            }
            catch (Exception ex)
            {
                _db.RollbackTransaction();
                _db.CloseConnection();

                lmy_wf_engine_result.Error = ErrorCodes.E002 + ex.Message;
            }

            return lmy_wf_engine_result;
        }

        public LMYWFEngineResult<LMYWFEngineTransition[]> GetAvailableTransitions(string workFlowName, string LMYWFEngineRequestID, string currentUser, string[] currentUserGroups)
        {
            LMYWFEngineResult<LMYWFEngineTransition[]> lmy_wf_engine_result = new LMYWFEngineResult<LMYWFEngineTransition[]>();

            CheckIsConfigured();
            CheckWorkFlowName(workFlowName);

            try
            {

                _db.OpenConnection();

                DataTable dbResult = _db.ExecuteSelect($"Select *  From LMYWFEngineRequests WHERE WorkFlowName = '{workFlowName}' AND LMYWFEngineRequestID = '{LMYWFEngineRequestID}' ");

                LMYWFEngineRequest request = LMYWFEngineUtilities.ConvertDataTableToRequestsObject(dbResult)[0];

                LMYWFEngineRequestTask[] userDelegatedTasks = GetUserDelegatedTasks(workFlowName, LMYWFEngineRequestID, currentUser);

                _db.CloseConnection();

                string[] currentUserGroupswithDelegatedUserGroups = userDelegatedTasks.Select(x => x.UserGroup).ToArray().Concat(currentUserGroups).ToArray();

                LMYWFEngineTransition[] transitions = GetAvailableTransitions(workFlowName, request.Status)
                    .Where(x => x.UserGroups.Any(ag => currentUserGroupswithDelegatedUserGroups.Any(ug => ug == ag)))
                    .ToArray();

                foreach (LMYWFEngineTransition transition in transitions)
                {
                    transition.LMYWFEngineRequestVersionID = request.LMYWFEngineRequestVersionID;
                }

                lmy_wf_engine_result.MethodResult = transitions;
            }
            catch (Exception ex)
            {
                _db.CloseConnection();

                lmy_wf_engine_result.Error = ErrorCodes.E003 + ex.Message;
            }

            return lmy_wf_engine_result;
        }

        public LMYWFEngineResult<bool> MoveNext(string workFlowName, string LMYWFEngineRequestID, string LMYWFEngineTransitionID,
         string LMYWFEngineRequestVersionID, string currentUser, string[] currentUserGroups, bool updateRequestCustomData = false, string requestCustomData = null)
        {
            LMYWFEngineResult<bool> lmy_wf_engine_result = new LMYWFEngineResult<bool>();

            CheckIsConfigured();
            CheckWorkFlowName(workFlowName);

            try
            {
                _db.OpenConnection();

                DataTable dbResult = _db.ExecuteSelect($@"Select *  From  LMYWFEngineRequests WHERE WorkFlowName = '{workFlowName}'
                    AND LMYWFEngineRequestID = '{LMYWFEngineRequestID}' ");

                if (dbResult.Rows.Count < 1)
                {
                    throw new Exception($"LMYWFEngineRequestID is not correct");
                }

                LMYWFEngineRequest request = LMYWFEngineUtilities.ConvertDataTableToRequestsObject(dbResult)[0];

                if (request.LMYWFEngineRequestVersionID != LMYWFEngineRequestVersionID)
                {
                    throw new Exception("Concurrent workflow process next attempt");
                }

                LMYWFEngineTransition takenTransition = _config.WorkFlows
                .Where(x => x.WorkFlowName == workFlowName)
                .Select(x => x.Transitions.First(transition => transition.LMYWFEngineTransitionID == LMYWFEngineTransitionID))
                .First();

                if (!takenTransition.CurrentStatuses.Any(x => x == request.Status))
                {
                    throw new Exception("Incorrect request transition attempt, status is not correct");
                }

                LMYWFEngineRequestTask[] userDelegatedTasks = GetUserDelegatedTasks(workFlowName, LMYWFEngineRequestID, currentUser);

                string[] currentUserGroupswithDelegatedUserGroups = userDelegatedTasks.Select(x => x.UserGroup).ToArray().Concat(currentUserGroups).ToArray();

                if (!takenTransition.UserGroups.Any(x => currentUserGroupswithDelegatedUserGroups.Any(g => g == x)))
                {
                    throw new Exception("Incorrect request transition attempt, current user groups not matching available transitions groups");
                }

                _db.BeginTransaction();

                //update request2
                LMYWFEngineDictionary updateRequestData = new LMYWFEngineDictionary();
                updateRequestData.Pairs = new LMYWFEngineKeyValuePair[5];

                updateRequestData.Pairs[0] = new LMYWFEngineKeyValuePair("Status", takenTransition.NextStatus);
                updateRequestData.Pairs[1] = new LMYWFEngineKeyValuePair("LMYWFEngineRequestVersionID", Guid.NewGuid().ToString());
                updateRequestData.Pairs[2] = new LMYWFEngineKeyValuePair("Modified_BY", currentUser);
                updateRequestData.Pairs[3] = new LMYWFEngineKeyValuePair("Modified_DATE", DateTime.Now);

                if (updateRequestCustomData)
                {
                    updateRequestData.Pairs[4] = new LMYWFEngineKeyValuePair("requestCustomData", requestCustomData);
                }

                LMYWFEngineDictionary updateRequestWhere = new LMYWFEngineDictionary()
                {
                    Pairs = new LMYWFEngineKeyValuePair[]{
                        new LMYWFEngineKeyValuePair("LMYWFEngineRequestID",LMYWFEngineRequestID)
                    }
                };

                _db.Update("LMYWFEngineRequests", updateRequestData, updateRequestWhere);

                //add log
                LMYWFEngineDictionary insertLogData = new LMYWFEngineDictionary()
                {
                    Pairs = new LMYWFEngineKeyValuePair[]{
                        new LMYWFEngineKeyValuePair("LMYWFEngineLogID",Guid.NewGuid().ToString()),
                        new LMYWFEngineKeyValuePair("WorkFlowName", workFlowName),
                        new LMYWFEngineKeyValuePair("LMYWFEngineRequestID", LMYWFEngineRequestID),
                        new LMYWFEngineKeyValuePair("OldStatus", request.Status),
                        new LMYWFEngineKeyValuePair("NewStatus", takenTransition.NextStatus),
                        new LMYWFEngineKeyValuePair("Created_BY", currentUser),
                        new LMYWFEngineKeyValuePair("Created_DATE", DateTime.Now),
                        new LMYWFEngineKeyValuePair("Modified_BY", currentUser),
                        new LMYWFEngineKeyValuePair("Modified_DATE", DateTime.Now)
                    }
                };

                _db.Insert("LMYWFEngineRequestStatusLogs", insertLogData);

                //Delete tasks
                _db.ExecuteNonQuery($@"Delete  From  LMYWFEngineRequestTasks WHERE WorkFlowName = '{workFlowName}' AND LMYWFEngineRequestID = '{LMYWFEngineRequestID}' ");

                //add tasks
                LMYWFEngineDictionary insertTaskData = new LMYWFEngineDictionary()
                {
                    Pairs = new LMYWFEngineKeyValuePair[]{
                            new LMYWFEngineKeyValuePair("LMYWFEngineTaskID",""),
                            new LMYWFEngineKeyValuePair("WorkFlowName", workFlowName),
                            new LMYWFEngineKeyValuePair("LMYWFEngineRequestID", LMYWFEngineRequestID),
                            new LMYWFEngineKeyValuePair("UserGroup", ""),
                            new LMYWFEngineKeyValuePair("Created_BY", currentUser),
                            new LMYWFEngineKeyValuePair("Created_DATE", DateTime.Now),
                            new LMYWFEngineKeyValuePair("Modified_BY", currentUser),
                            new LMYWFEngineKeyValuePair("Modified_DATE", DateTime.Now)
                        }
                };

                string tasksGroups = "";
                foreach (LMYWFEngineTransition transition in GetAvailableTransitions(workFlowName, takenTransition.NextStatus))
                {
                    foreach (var userGroup in transition.UserGroups)
                    {
                        //to prevent add same task to same group of users
                        if (!tasksGroups.Contains($"[{userGroup}]"))
                        {

                            tasksGroups += $"[{userGroup}]";
                            insertTaskData.Pairs[0].Value = Guid.NewGuid().ToString();
                            insertTaskData.Pairs[3].Value = userGroup;
                            _db.Insert("LMYWFEngineRequestTasks", insertTaskData);
                        }
                    }
                }

                _db.CommitTransaction();
                _db.CloseConnection();

                lmy_wf_engine_result.MethodResult = true;
            }
            catch (Exception ex)
            {
                _db.RollbackTransaction();
                _db.CloseConnection();

                lmy_wf_engine_result.Error = ErrorCodes.E004 + ex.Message;
            }

            return lmy_wf_engine_result;
        }

        public LMYWFEngineResult<LMYWFEngineRequestTask[]> GetTasks(string workFlowName, string currentUser, string[] currentUserGroups, int page, int pageSize, out int totalRecords)
        {
            LMYWFEngineResult<LMYWFEngineRequestTask[]> lmy_wf_engine_result = new LMYWFEngineResult<LMYWFEngineRequestTask[]>();
            totalRecords = 0;

            CheckIsConfigured();
            CheckWorkFlowName(workFlowName);

            try
            {
                _db.OpenConnection();
                string whereStatment = @$"WHERE tasks.WorkFlowName = '{workFlowName}' 
                        AND 
                        (
                            tasks.UserGroup in ({LMYWFEngineUtilities.JoinStringArrayForSqlQuery(currentUserGroups)}) 
                            OR tasks.DelegatedToUser='{currentUser}'
                        )";

                totalRecords = (int)_db.ExecuteScalar($@"SELECT count(tasks.LMYWFEngineTaskID) From LMYWFEngineRequestTasks  tasks {whereStatment}");

                DataTable dbResult = _db.ExecuteSelect($@"Select 
                        tasks.LMYWFEngineTaskID as tasks_LMYWFEngineTaskID,
                        tasks.WorkFlowName as tasks_WorkFlowName,
                        tasks.LMYWFEngineRequestID  as tasks_LMYWFEngineRequestID,
                        tasks.UserGroup as tasks_UserGroup,
                        tasks.DelegatedToUser as tasks_DelegatedToUser,
                        tasks.Created_BY as tasks_Created_BY,
                        tasks.Created_DATE as tasks_Created_DATE,
                        tasks.Modified_BY as tasks_Modified_BY,
                        tasks.Modified_DATE as tasks_Modified_DATE,
                        requests.LMYWFEngineRequestID as requests_LMYWFEngineRequestID,
                        requests.LMYWFEngineRequestVersionID as requests_LMYWFEngineRequestVersionID,
                        requests.WorkFlowName as requests_WorkFlowName,
                        requests.Status as requests_Status,
                        requests.RequestCustomData as requests_RequestCustomData,
                        requests.Created_BY as requests_Created_BY,
                        requests.Created_DATE as requests_Created_DATE,
                        requests.Modified_BY as requests_Modified_BY,
                        requests.Modified_DATE as requests_Modified_DATE
                        From 
                        LMYWFEngineRequestTasks tasks inner join 
                        LMYWFEngineRequests requests ON requests.LMYWFEngineRequestID = tasks.LMYWFEngineRequestID
                        {whereStatment}
                        ORDER BY tasks.Created_DATE desc
                        OFFSET {(page - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY;"
                    );

                _db.CloseConnection();

                LMYWFEngineRequestTask[] tasks = LMYWFEngineUtilities.ConvertDataTableToRequestTasksObject(dbResult);

                foreach (var task in tasks)
                {
                    task.AvailableTransitions = GetAvailableTransitions(workFlowName, task.Request.Status);

                    foreach (LMYWFEngineTransition transition in task.AvailableTransitions)
                    {
                        transition.LMYWFEngineRequestVersionID = task.Request.LMYWFEngineRequestVersionID;
                    }
                }

                lmy_wf_engine_result.MethodResult = tasks;
            }
            catch (Exception ex)
            {
                _db.CloseConnection();

                lmy_wf_engine_result.Error = ErrorCodes.E005 + ex.Message;
            }

            return lmy_wf_engine_result;
        }

        public LMYWFEngineResult<LMYWFEngineRequestStatusLog[]> GetRequestLogs(string workFlowName, string LMYWFEngineRequestID)
        {
            LMYWFEngineResult<LMYWFEngineRequestStatusLog[]> lmy_wf_engine_result = new LMYWFEngineResult<LMYWFEngineRequestStatusLog[]>();

            CheckIsConfigured();
            CheckWorkFlowName(workFlowName);

            try
            {
                _db.OpenConnection();

                DataTable dbResult = _db.ExecuteSelect($@"Select * From LMYWFEngineRequestStatusLogs  WHERE WorkFlowName = '{workFlowName}' AND LMYWFEngineRequestID = '{LMYWFEngineRequestID}' ");
                _db.CloseConnection();

                if (dbResult.Rows.Count < 1)
                {
                    throw new Exception($"WorkFlowName or LMYWFEngineRequestID is not correct");
                }

                LMYWFEngineRequestStatusLog[] logs = LMYWFEngineUtilities.ConvertDataTableToRequestStatusLogsObject(dbResult);

                lmy_wf_engine_result.MethodResult = logs;
            }
            catch (Exception ex)
            {
                _db.CloseConnection();

                lmy_wf_engine_result.Error = ErrorCodes.E006 + ex.Message;
            }

            return lmy_wf_engine_result;
        }

        public LMYWFEngineResult<bool> DelegateTask(string workFlowName, string LMYWFEngineTaskID, string delegatedToUser, string currentUser)
        {
            LMYWFEngineResult<bool> lmy_wf_engine_result = new LMYWFEngineResult<bool>();

            CheckIsConfigured();
            CheckWorkFlowName(workFlowName);

            try
            {
                _db.BeginTransaction();

                //update task
                LMYWFEngineDictionary updateRequestData = new LMYWFEngineDictionary()
                {
                    Pairs = new LMYWFEngineKeyValuePair[] {
                        new LMYWFEngineKeyValuePair("DelegatedToUser", delegatedToUser),
                        new LMYWFEngineKeyValuePair("Modified_BY", currentUser),
                        new LMYWFEngineKeyValuePair("Modified_DATE", DateTime.Now)
                    }
                };

                LMYWFEngineDictionary updateRequestWhere = new LMYWFEngineDictionary()
                {
                    Pairs = new LMYWFEngineKeyValuePair[]{
                        new LMYWFEngineKeyValuePair("LMYWFEngineTaskID",LMYWFEngineTaskID)
                    }
                };


                _db.Update("LMYWFEngineRequestTasks", updateRequestData, updateRequestWhere);

                _db.CommitTransaction();

                lmy_wf_engine_result.MethodResult = true;
            }
            catch (Exception ex)
            {
                _db.RollbackTransaction();
                _db.CloseConnection();
                lmy_wf_engine_result.Error = ErrorCodes.E007 + ex.Message;
            }

            return lmy_wf_engine_result;
        }

        private LMYWFEngineRequestTask[] GetUserDelegatedTasks(string workFlowName, string LMYWFEngineRequestID, string currentUser)
        {
            _db.OpenConnection();

            DataTable result = _db.ExecuteSelect($@"Select 
                tasks.LMYWFEngineTaskID as tasks_LMYWFEngineTaskID,
                tasks.WorkFlowName as tasks_WorkFlowName,
                tasks.LMYWFEngineRequestID  as tasks_LMYWFEngineRequestID,
                tasks.UserGroup as tasks_UserGroup,
                tasks.DelegatedToUser as tasks_DelegatedToUser,
                tasks.Created_BY as tasks_Created_BY,
                tasks.Created_DATE as tasks_Created_DATE,
                tasks.Modified_BY as tasks_Modified_BY,
                tasks.Modified_DATE as tasks_Modified_DATE
                From 
                LMYWFEngineRequestTasks tasks     
                WHERE tasks.WorkFlowName = '{workFlowName}' 
                AND tasks.LMYWFEngineRequestID='{LMYWFEngineRequestID}'
                AND tasks.DelegatedToUser='{currentUser}'"
            );

            _db.CloseConnection();

            return LMYWFEngineUtilities.ConvertDataTableToRequestTasksObject(result);
        }

        private LMYWFEngineTransition[] GetAvailableTransitions(string workFlowName, string requestStatus)
        {
            var transitions = _config.WorkFlows
                .Where(x => x.WorkFlowName == workFlowName)
                .SelectMany(x => x.Transitions.Where(transition => transition.CurrentStatuses.Any(currentStatus => currentStatus == requestStatus)))
                .Select(x => new LMYWFEngineTransition()
                {
                    LMYWFEngineTransitionID = x.LMYWFEngineTransitionID,
                    LMYWFEngineRequestVersionID = x.LMYWFEngineRequestVersionID,
                    UserGroups = x.UserGroups,
                    TransitionName = x.TransitionName,
                    CurrentStatuses = x.CurrentStatuses,
                    NextStatus = x.NextStatus,
                    TransitionPageName = x.TransitionPageName,
                    TransitionCustomData = x.TransitionCustomData,
                })
                .ToArray();


            return transitions;
        }

        private void CheckIsConfigured()
        {
            if (!isConfigured)
            {
                throw new Exception("WorkFlow engine is not configured correctly, make sure to call UseLMYWFEngine on startup");
            }
        }

        private void CheckWorkFlowName(string workFlowName)
        {
            bool workFlowFound = false;

            foreach (var workFlow in _config.WorkFlows)
            {
                if (workFlow.WorkFlowName == workFlowName)
                {
                    workFlowFound = true;
                }
            }

            if (!workFlowFound)
            {
                throw new Exception("work flow name is not found in configuration file");
            }
        }
    }
}