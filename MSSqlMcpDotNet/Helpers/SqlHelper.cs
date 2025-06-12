using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace MSSqlMcpDotNet;

public static class SqlHelper
{

    public static List<Dictionary<string, object>> DataReaderToDictionaryList(SqlDataReader reader)
    {
        var results = new List<Dictionary<string, object>>();

        while (reader.Read())
        {
            var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                row[reader.GetName(i)] = value;
            }
            results.Add(row);
        }
        return results;
    }


    public static async Task<string> ExecSql(string databaseName, string qry, Dictionary<string, string> parameters)
    {
        string connectionString = ConfigHelper.LoadConnectionString();
        connectionString += $"database={databaseName};";  
        using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            var command = new SqlCommand(qry, connection);

            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value);
            }
            
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
            using (var reader = await command.ExecuteReaderAsync())
            {
                results = DataReaderToDictionaryList(reader);
            }
            return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
        }
    }


}
