If not exists (select name from sysobjects where name = 'LMYWFEngineRequestStatusLogs')
Create table LMYWFEngineRequestStatusLogs
(
    LMYWFEngineLogID nvarchar(128) not null PRIMARY KEY,
    WorkFlowName nvarchar(max) not null,
    LMYWFEngineRequestID nvarchar(128),
    OldStatus nvarchar(max),
    NewStatus nvarchar(max),
    Created_BY nvarchar(max),
    Created_DATE datetime,
    Modified_BY nvarchar(max),
    Modified_DATE datetime,
    CONSTRAINT FK_LMY_Logs_Requests FOREIGN KEY (LMYWFEngineRequestID) REFERENCES LMYWFEngineRequests(LMYWFEngineRequestID)
);
