namespace MSSqlMcpDotNet.Tests;

public class ConfigHelperTest
{
    string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sqlserver.secured.json");

    private void DeleteConfigFile()
    {
        if (File.Exists(configFilePath))
        {
            File.Delete(configFilePath);
        }
    }

    private void CreateEmptyConfigFile()
    {
        // Ensure the config file is deleted before creating a new one
        DeleteConfigFile();
        // Create an empty config file
        using (var fileStream = File.Create(configFilePath))
        {
            // Create an empty file
            fileStream.Close();
        }
    }

    [Fact]
    public void ConfigFileExists_ShouldReturnTrue_WhenConfigFileExists()
    {

        CreateEmptyConfigFile();
        bool configFileExists = MSSqlMcpDotNet.ConfigHelper.ConfigFileExists();
        Assert.True(configFileExists);
        File.Delete(configFilePath);
    }

    [Fact]
    public void ConfigFileExists_ShouldReturnFalse_WhenConfigFileDoesNotExist()
    {
        DeleteConfigFile();
        bool configFileExists = MSSqlMcpDotNet.ConfigHelper.ConfigFileExists();
        Assert.False(configFileExists);
    }


    //test wrong config file is not readable
    [Fact]
    public void ConfigFileInvalid_ThrowsException()
    {
        // Create a file with invalid JSON content
        CreateEmptyConfigFile();
        Assert.ThrowsAny<Exception>(() => ConfigHelper.LoadConnectionString());
        DeleteConfigFile();
    }

    [Fact]
    public static void TestSaveLoadConnectionStringTrusted()
    {
        string instancia = "test_server";
        ConfigHelper.SaveConnectionStringTrusted(instancia);
        string loadedCnxStr = ConfigHelper.LoadConnectionString();
        string cnxStr = $"Server={instancia};Integrated Security=True;TrustServerCertificate=True;";
        Assert.Equal(cnxStr, loadedCnxStr);
    }

    [Fact]
    public static void TestSaveLoadConnectionString()
    {
        string instancia = "test_server";
        string usuario = "test_user";
        string password = "test_password";
        ConfigHelper.SaveConnectionString(instancia, usuario, password);
        string loadedCnxStr = ConfigHelper.LoadConnectionString();
        string cnxStr = $"Server={instancia};User Id={usuario};Password={password};TrustServerCertificate=True;";
        Assert.Equal(cnxStr, loadedCnxStr);
    }
}