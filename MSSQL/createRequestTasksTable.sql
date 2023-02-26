If not exists (select name from sysobjects where name = 'LMYWFEngineRequestTasks')
Create table LMYWFEngineRequestTasks
(
    LMYWFEngineTaskID nvarchar(128) not null PRIMARY KEY,
    WorkFlowName nvarchar(max) not null,
    LMYWFEngineRequestID nvarchar(128) ,
    UserGroup nvarchar(max) null,
    DelegatedToUser nvarchar(max) null,
    Created_BY nvarchar(max),
    Created_DATE datetime,
    Modified_BY nvarchar(max),
    Modified_DATE datetime,
    CONSTRAINT FK_LMY_Tasks_Requests FOREIGN KEY (LMYWFEngineRequestID) REFERENCES LMYWFEngineRequests(LMYWFEngineRequestID)
);
 