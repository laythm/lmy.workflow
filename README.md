
# lmy.workflow

LMY.workflow is a powerful workflow engine that simplifies simple and complex workflow problems.

LMY.workflow provides a simple and efficient way to develop workflows in your system.

Whether you are dealing with simple or complex workflows, LMY.workflow can handle it all.

it is built using .net 6



## How To Use

1 - Include ```LMY.WorkFlow``` in your project.

2 - build your workflow configuration file (check workflow sample ```wf.config.json```).

3 - add below to program.cs 

```
builder.Services
.AddLMYWFEngineMSSQL()
.AddLMYWFEngine();

builder.UseLMYWFEngine(o =>
{
    o.WorkFlowsConfigFilePath = @"C:\wf.config.json";
    o.DBConnectionString = @"Server=.\SQLEXPRESS;Database=testdb;Integrated Security=SSPI;";
});
```
4 - once you have included the ```LMYEngine```, you can now inject the ```ILMYWFEngine``` whenever you need it, making it available for use throughout your application
```
ILMYWFEngine lmyWFEngine;
public TestController(ILMYWFEngine _lmyWFEngine)
{
    lmyWFEngine= _lmyWFEngine;
}
```

5 - now you can start work flow like below

```
var result = lmyWFEngine.StartNew("LeaveRequestWF", "user1");
var LMYWFEngineRequestID = result.MethodResult;//started worklfow id
```

6 -you can get available transitions for specific workflow (you need to pass workflowid and current user groups ) 

```
var transitions =lmyWFEngine.GetAvailableTransitions("LeaveRequestWF", LMYWFEngineRequestID, "user1", new string[] { "DataEntry", "Leader" });

var LMYWFEngineRequestID = result.MethodResult;
```

7 - process taken transition  
```
var moveNextResult = lmyWFEngine.MoveNext("LeaveRequestWF", LMYWFEngineRequestID, transitions.MethodResult[0].LMYWFEngineTransitionID, transitions.MethodResult[0].LMYWFEngineRequestVersionID, "user1", new string[] { "DataEntry", "Leader" }, true, "{you can include here whatever data you want}");
```


8 - you can get current user tasks (based on provided user groups) 
```
int total = 0;

var tasks = lmyWFEngine.GetTasks("LeaveRequestWF", "user1", new string[] { "DataEntry", "Leader" }, 1, 10, out total);
```

0 - sample for a work flow drawing
```
{
  "WorkFlows": [
    {
      "WorkFlowName": "LeaveRequestWF",
      "Transitions": [
        {
          "UserGroups": [
            "DataEntry"
          ],
          "TransitionName": "Save As Draft",
          "CurrentStatuses": [
            null,
            "DataEntryDraft",
            "ReturnedFromLeader",
            "ReturnedByManagerToDataEntry"
          ],
          "NextStatus": "DataEntryDraft",
          "TransitionPageName": "SavePage",
          "TransitionCustomData": {}
        },
        {
          "UserGroups": [
            "DataEntry"
          ],
          "TransitionName": "Send To Leader",
          "CurrentStatuses": [
            null,
            "DataEntryDraft",
            "ReturnedFromLeader",
            "ReturnedByManagerToDataEntry"
          ],
          "NextStatus": "SentToLeader",
          "TransitionPageName": "SavePage",
          "TransitionCustomData": {}
        },
        {
          "UserGroups": [
            "Leader"
          ],
          "TransitionName": "Return",
          "CurrentStatuses": [
            "SentToLeader",
            "LeaderDraft"
          ],
          "NextStatus": "ReturnedFromLeader",
          "TransitionPageName": "LeaderApprovalPage",
          "TransitionCustomData": {}
        },
        {
          "UserGroups": [
            "Leader"
          ],
          "TransitionName": "Reject",
          "CurrentStatuses": [
            "SentToLeader",
            "LeaderDraft"
          ],
          "NextStatus": "RejectedByLeader",
          "TransitionPageName": "LeaderApprovalPage",
          "TransitionCustomData": {}
        },
        {
          "UserGroups": [
            "Leader"
          ],
          "TransitionName": "Edit And Save",
          "CurrentStatuses": [
            "SentToLeader",
            "LeaderDraft",
            "ReturnedByManagerToLeader"
          ],
          "NextStatus": "LeaderDraft",
          "TransitionPageName": "LeaderApprovalPage",
          "TransitionCustomData": {}
        },
        {
          "UserGroups": [
            "Leader"
          ],
          "TransitionName": "Approve",
          "CurrentStatuses": [
            "SentToLeader",
            "LeaderDraft",
            "ReturnedByManagerToLeader"
          ],
          "NextStatus": "SentToManager",
          "TransitionPageName": "LeaderApprovalPage",
          "TransitionCustomData": {}
        },
        {
          "UserGroups": [
            "Manager"
          ],
          "TransitionName": "Edit And Save",
          "CurrentStatuses": [
            "SentToManager",
            "ManagerDraft"
          ],
          "NextStatus": "ManagerDraft",
          "TransitionPageName": "ManagerApprovalPage",
          "TransitionCustomData": {}
        },
        {
          "UserGroups": [
            "Manager"
          ],
          "TransitionName": "Return To Leader",
          "CurrentStatuses": [
            "SentToManager",
            "ManagerDraft"
          ],
          "NextStatus": "ReturnedByManagerToLeader",
          "TransitionPageName": "ManagerApprovalPage",
          "TransitionCustomData": {}
        },
        {
          "UserGroups": [
            "Manager"
          ],
          "TransitionName": "Return To Data Entry",
          "CurrentStatuses": [
            "SentToManager",
            "ManagerDraft"
          ],
          "NextStatus": "ReturnedByManagerToDataEntry",
          "TransitionPageName": "ManagerApprovalPage",
          "TransitionCustomData": {}
        },
        {
          "UserGroups": [
            "Manager"
          ],
          "TransitionName": "Reject",
          "CurrentStatuses": [
            "SentToManager",
            "ManagerDraft"
          ],
          "NextStatus": "RejectedByManager",
          "TransitionPageName": "ManagerApprovalPage",
          "TransitionCustomData": {}
        },
        {
          "UserGroups": [
            "Manager"
          ],
          "TransitionName": "Approve",
          "CurrentStatuses": [
            "SentToManager",
            "ManagerDraft"
          ],
          "NextStatus": "ApprovedByManager",
          "TransitionPageName": "ManagerApprovalPage",
          "TransitionCustomData": {}
        }
      ]
    }
  ]
}
 
```
