using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

namespace MSSqlMcpDotNet.Services;



[McpServerToolType]
public partial class MsSqlDbInfo
{
    private readonly IConfiguration _configuration;

    private readonly ILogger<MsSqlDbInfo> _logger;


    public MsSqlDbInfo(IConfiguration configuration, ILogger<MsSqlDbInfo> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }


    


    [McpServerTool, Description("Retrieves metadata for all databases in the SQL Server instance, including database names and properties useful for understanding database configuration.")]
    public async Task<string> Databases()
    {
        string methodName = MethodBase.GetCurrentMethod().Name;
        return await GetResponse(methodName,"master", qryDatabases, new Dictionary<string, string>());
    }





    [McpServerTool, Description("Retrieves all user-defined tables in the specified database, with schema name, table name, and relevant table metadata, to assist in SQL query generation.")]
    public async Task<string> DatabaseTables(
        [Description("Name of the database to query.")] string databaseName = "master"
        )
    {
        string methodName = MethodBase.GetCurrentMethod().Name;
        return await GetResponse(methodName, databaseName, qryTables, new Dictionary<string, string>());
    }





    [McpServerTool, Description("Retrieves all user-defined views in the specified database, with schema name, view name, and relevant view metadata, to assist in SQL query generation.")]
    public async Task<string> DatabaseViews(
        [Description("Name of the database.")] string databaseName = "master"
    )
    {
        string methodName = MethodBase.GetCurrentMethod().Name;
        return await GetResponse(methodName, databaseName, qryViews, new Dictionary<string, string>());
    }





    [McpServerTool, Description("Provides detailed metadata about the output columns of a specified view, including column names, data types, nullability, and column order, to assist in SQL query generation.")]
    public async Task<string> ViewColumns(
        [Description("Name of the database.")] string databaseName = "master",
        [Description("Schema of the view.")] string schema = "dbo",
        [Description("Name of the view.")] string viewName = ""
    )
    {
        string methodName = MethodBase.GetCurrentMethod().Name;
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("@schema", schema);
        parameters.Add("@viewName", viewName);
        return await GetResponse(methodName, databaseName, qryViewColumns, parameters);
    }



    [McpServerTool, Description("Provides detailed metadata about the output columns of a specified table, including column names, data types, nullability, and column order, to assist in SQL query generation.")]
    public async Task<string> TableColumns(
        [Description("Name of the database.")] string databaseName = "master",
        [Description("Schema of the table.")] string schema = "dbo",
        [Description("Name of the table.")] string tableName = ""
    )
    {
        string methodName = MethodBase.GetCurrentMethod().Name;
        Dictionary<string,string> parameters = new Dictionary<string, string>();
        parameters.Add("@schema", schema);
        parameters.Add("@tableName", tableName);
        return await GetResponse(methodName, databaseName, qryTableColumns, parameters);
    }




    [McpServerTool, Description("Retrieves metadata about foreign key relationships for a specified table, including source columns, target (referenced) table and columns, update and delete actions, and a list of tables that reference the specified table via foreign keys.")]
    public async Task<string> TableReferentialIntegrity(
        [Description("Name of the database.")] string databaseName = "master",
        [Description("Schema of the table.")] string schema = "",
        [Description("Name of the table.")] string tableName = ""
        )
    {
        string methodName = MethodBase.GetCurrentMethod().Name;
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("@schema", schema);
        parameters.Add("@tableName", tableName);
        return await GetResponse(methodName, databaseName, qryTableReferentialIntegrity, parameters);
    }







    private async Task<string> GetResponse(string methodName, string databaseName, string qry, Dictionary<string, string> parameters)
    {
        string response = "";
        try
        {
            response = await SqlHelper.ExecSql(databaseName, qry, parameters);
            _logger.LogInformation("Executed {methodName} in database {databaseName} using query :\n {Query}", methodName, qry, databaseName);
        }
        catch (Exception ex)
        {
            response = JsonSerializer.Serialize(new { error = $"Error executing {methodName} in database {databaseName}." }, new JsonSerializerOptions { WriteIndented = true });
            _logger.LogError(ex, "Error executing {methodName} in database {databaseName} using query :\n {Query}", methodName, qry, databaseName);
        }
        return response;
    }
}