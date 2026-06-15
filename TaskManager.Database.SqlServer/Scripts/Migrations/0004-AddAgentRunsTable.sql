CREATE TABLE [dbo].[AgentRuns]
(
    [Id]            [uniqueidentifier]  NOT NULL,
    [TaskId]        [uniqueidentifier]  NOT NULL,
    [Status]        [nvarchar](50)      NOT NULL,
    [Branch]        [nvarchar](255)     NULL,
    [PrUrl]         [nvarchar](1000)    NULL,
    [CostUsd]       [decimal](10,4)     NOT NULL CONSTRAINT [DF_AgentRuns_CostUsd] DEFAULT (0),
    [CreatedAt]     datetimeoffset(7)   NOT NULL,
    [UpdatedAt]     datetimeoffset(7)   NOT NULL,

    CONSTRAINT [PK_AgentRuns] PRIMARY KEY (Id)
);
GO

CREATE INDEX [IX_AgentRuns_TaskId] ON [dbo].[AgentRuns] ([TaskId]);
