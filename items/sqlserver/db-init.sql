IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'TaskManager')
BEGIN
	CREATE DATABASE [TaskManager]
	COLLATE Polish_100_CI_AS;
END
GO