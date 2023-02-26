If not exists (select name from sysobjects where name = 'LMYWFEngineRequests')
Create table LMYWFEngineRequests
(
    LMYWFEngineRequestID nvarchar(128) not null PRIMARY KEY,
    LMYWFEngineRequestVersionID nvarchar(128) null,
    WorkFlowName nvarchar(max) not null,
    [Status] nvarchar(max) null,
    RequestCustomData nvarchar(max) null,
    Created_BY nvarchar(max),
    Created_DATE datetime,
    Modified_BY nvarchar(max),
    Modified_DATE datetime,
);
 