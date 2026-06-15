CREATE TABLE [dbo].[AgentRuns]
(
    [Id]            [uniqueidentifier]  NOT NULL,
    [TaskId]        [uniqueidentifier]  NOT NULL,
    [Status]        [nvarchar](50)      NOT NULL,
    [Branch]        [nvarchar](255)     NULL,
    [PrUrl]         [nvarchar](1000)    NULL,
    [CostUsd]       [decimal](10,4)     NOT NULL,
    [CreatedAt]     datetimeoffset(7)   NOT NULL,
    [UpdatedAt]     datetimeoffset(7)   NOT NULL,

    CONSTRAINT [PK_AgentRuns] PRIMARY KEY (Id)
);
