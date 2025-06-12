/*
----------------------------------------------------------------------------------------------------
Use this script to create a login for the MCP user in SQL Server.
Replace <yourStrongPassword123!> with the desired password for the login.
----------------------------------------------------------------------------------------------------
*/


USE [master]
GO
CREATE LOGIN [mcpUser] 
WITH 
	PASSWORD=N'<yourStrongPassword123!>', 
	DEFAULT_DATABASE=[master], 
	CHECK_EXPIRATION=OFF, 
	CHECK_POLICY=OFF
GO
GRANT CONNECT ANY DATABASE TO [mcpUser]
GO
GRANT CONNECT SQL TO [mcpUser]
GO
GRANT VIEW ANY DATABASE TO [mcpUser]
GO
GRANT VIEW ANY DEFINITION TO [mcpUser]
GO
GRANT VIEW ANY ERROR LOG TO [mcpUser]
GO
GRANT VIEW ANY PERFORMANCE DEFINITION TO [mcpUser]
GO
GRANT VIEW SERVER STATE TO [mcpUser]
GO


-- Read permission in the specific database
USE [<MyDatabase>]
GO
CREATE USER [mcpUser] FOR LOGIN [mcpUser]
GO
USE [<MyDatabase>]
GO
ALTER ROLE [db_datareader] ADD MEMBER [mcpUser]
GO
