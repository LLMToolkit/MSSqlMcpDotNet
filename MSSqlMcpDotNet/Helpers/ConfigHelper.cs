using MSSqlMcpDotNet.Models;
using System.Text.Json;

namespace MSSqlMcpDotNet;

public static class ConfigHelper
{

    /// <summary>    
    /// Checks if the configuration file exists.
    /// </summary>
    /// <returns>
    /// A boolean indicating if the configuration file exists.
    /// </returns>
    public static bool ConfigFileExists()
    {
        string ConfigFilePath = GetConfigFilePath();
        return File.Exists(ConfigFilePath);
    }


    /// <summary>
    /// Loads the connection string from the saved configuration file.
    /// </summary>
    /// <returns>A decrypted connection string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no saved configuration is found.</exception>
    public static string LoadConnectionString()
    {
        if (!ConfigFileExists())
            throw new InvalidOperationException("No saved configuration found.");

        string ConfigFilePath = GetConfigFilePath();
        var config = JsonSerializer.Deserialize<SqlServerConfig>(File.ReadAllText(ConfigFilePath));
        return CryptoHelper.Decrypt(config.EncryptedConnectionString);
    }


    public static void SaveConnectionString(string instancia, string usuario, string password)
    {
        string cnxStr = $"Server={instancia};User Id={usuario};Password={password};TrustServerCertificate=True;";
        SaveCnxStringInternal(cnxStr);
    }


    public static void SaveConnectionStringTrusted(string instancia)
    {
        string cnxStr = $"Server={instancia};Integrated Security=True;TrustServerCertificate=True;";
        SaveCnxStringInternal(cnxStr);
    }


    private static string GetConfigFilePath()
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sqlserver.secured.json");
    }

    private static void SaveCnxStringInternal(string cnxStr)
    {
        string ConfigFilePath = GetConfigFilePath();
        var encrypted = CryptoHelper.Encrypt(cnxStr);
        var config = new SqlServerConfig { EncryptedConnectionString = encrypted };
        File.WriteAllText(ConfigFilePath, JsonSerializer.Serialize(config));
    }


}