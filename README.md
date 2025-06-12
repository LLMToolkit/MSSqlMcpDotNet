# MsSqlMcpDotNet

## Microsoft SQL Server MCP Server

**MsSqlMcpDotNet** is a **Model Context Protocol (MCP) server** that enables secure interaction with Microsoft SQL Server databases.
It allows **AI assistants**—such as **GitHub Copilot Agent**—to generate and execute SQL queries in a controlled and safe manner.

## Features

* Implements an **MCP server** for Microsoft SQL Server.
* **SQL injection protection** via parameterized queries.
* **Compatible with GitHub Copilot Agent**.
* Retrieves server-level metadata:

  * Databases
* Retrieves detailed schema and database metadata:

  * Tables
  * Views
  * Columns
  * Keys (Primary / Foreign)
* Stores the database **connection string securely** using **Windows DPAPI** (Data Protection API).
* Fully **asynchronous database operations** using `async/await`.
* **Structured logging** with **Serilog**.

## Prerequisites

* [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download) or later to build and run the project.

## SQL Server Secure Configuration

A SQL script is provided to create a dedicated **secure login** in SQL Server for use by the MCP server.
The MCP login is granted **minimal read-only permissions** on the target database.

> ⚠️ **IMPORTANT:**
> The MCP user is explicitly restricted from writing or modifying data in the database or SQL Server instance. This ensures that the MCP server can **only read data** and cannot alter the database in any way.

```sql
/* ----------------------------------------------------------------------------------------------
   Use this script to create a login for the MCP user in SQL Server.
   Replace <yourStrongPassword123!> with the desired password for the login.
   ---------------------------------------------------------------------------------------------- */

USE [master]
GO
CREATE LOGIN [mcpUser]  
WITH  
    PASSWORD = N'<yourStrongPassword123!>',  
    DEFAULT_DATABASE = [master],  
    CHECK_EXPIRATION = OFF,  
    CHECK_POLICY = OFF
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
ALTER ROLE [db_datareader] ADD MEMBER [mcpUser]
GO
```

## MCP Server Configuration

To set the connection string and configure the MCP server, simply run `MSSqlMcpDotNet.exe`.
The program will prompt for:

* SQL Server instance name
* Trusted connection option
* User credentials (if required)

This process will generate a `sqlserver.secured.json` file containing the **connection string encrypted using DPAPI**.

If you wish to change the access credentials, simply delete the existing `sqlserver.secured.json` file and repeat the configuration process.

## Visual Studio Code Configuration

Edit your `App.Settings` file and add the MCP server configuration under the `mcp` section.
Remember to start the server before using it from Visual Studio Code.

```json
"mcp": {
    "servers": {
        "MSSqlMcpDotNet": {
            "type": "stdio",
            "command": "C:\\bin\\mcp\\MSSqlMcpDotNet\\MSSqlMcpDotNet.exe",
            "args": []
        }
    }
}
```

## `mcp.json`

This file is used in **Visual Studio 2022** or in any **MCP Client Application**.

The `mcp.json` file defines the MCP server configuration.

```json
{
  "servers": {
    "MSSqlMcpDotNet": {
      "type": "stdio",
      "command": "C:\\bin\\mcp\\MSSqlMcpDotNet\\MSSqlMcpDotNet.exe",
      "args": []
    }
  }
}
```

## License

This project is licensed under the **MIT License**.
See the [LICENSE](LICENSE) file for more details.
