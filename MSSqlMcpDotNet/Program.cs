using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;


namespace MSSqlMcpDotNet;

class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();
        try
        {
            Log.Information("Starting MSSqlMcpDotNet");

            if (!ConfigHelper.ConfigFileExists())
            {
                SetupHelper.SetupDatabaseConnection();
                return;
            }

            if (InitializeLogPath(out var logPath)) 
                return;

            var builder = Host.CreateApplicationBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(builder.Services.BuildServiceProvider())
                .Enrich.FromLogContext()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                .CreateLogger();


            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger);

            builder.Services
                .AddMcpServer()
                .WithStdioServerTransport()
                .WithToolsFromAssembly();

            await builder.Build().RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally 
        {
            Log.CloseAndFlush();
        }

    }


    private static bool InitializeLogPath(out string logPath)
    {
        string? exeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        if (exeDirectory == null)
        {
            Log.Fatal("Could not determine the executable directory.");
            logPath = null;
            return true;
        }
        logPath = Path.Combine(exeDirectory, "log\\MSSqlMcpDotNet.Log");


        if (!Directory.Exists(Path.GetDirectoryName(logPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));
        }

        return false;
    }
}