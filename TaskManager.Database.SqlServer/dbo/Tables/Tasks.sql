CREATE TABLE [dbo].[Tasks]
(
    [Id]                [uniqueidentifier]      NOT NULL,
    [ClusteredId]       [bigint] IDENTITY (1,1) NOT NULL,
    [Name]              [nvarchar](255)         NOT NULL,
    [Description]       [nvarchar](1275)        NOT NULL,
    [Status]            [int]                   NOT NULL,
    [CreationDate]      datetimeoffset(7)       NOT NULL,
    [LastUpdateDate]    datetimeoffset(7)       NOT NULL,

    CONSTRAINT [PK_Tasks] PRIMARY KEY (Id)
);