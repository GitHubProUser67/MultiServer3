using CyberBackendLibrary.GeoLocalization;
using CustomLogger;
using DatabaseMiddleware.HTTPEngine;
using DatabaseMiddleware.SQLiteEngine;
using Newtonsoft.Json.Linq;
using System.Runtime;

public static class DatabaseMiddlewareServerConfiguration
{
    public static string DatabaseAccessKey { get; set; } = "9hK8qm93OQBuuFYE7q8Tfd0YNlFOM3hRMXf1qFhRbqg="; // Base64 only.
    public static string AuthenticationsList { get; set; } = $"{Directory.GetCurrentDirectory()}/static/DatabaseMiddleware/AuthenticationsList.json";
    public static List<string>? DbFiles { get; set; } = new List<string> {  $"medius_database,{Directory.GetCurrentDirectory()}/static/DatabaseMiddleware/medius_database.sqlite" };
    public static List<string>? BannedIPs { get; set; }

    /// <summary>
    /// Tries to load the specified configuration file.
    /// Throws an exception if it fails to find the file.
    /// </summary>
    /// <param name="configPath"></param>
    /// <exception cref="FileNotFoundException"></exception>
    public static void RefreshVariables(string configPath)
    {
        // Make sure the file exists
        if (!File.Exists(configPath))
        {
            LoggerAccessor.LogWarn("Could not find the dbmiddleware.json file, using server's default.");
            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

            DatabaseAccessKey = GetValueOrDefault(config, "database_accesskey", DatabaseAccessKey);
            AuthenticationsList = GetValueOrDefault(config, "authenticationslist", AuthenticationsList);
            // Deserialize DbConnectionsArray if it exists
            try
            {
                JArray DbFilesArray = config.DbFiles;
                if (DbFilesArray != null)
                    DbFiles = DbFilesArray.ToObject<List<string>>();
            }
            catch
            {

            }
            // Deserialize BannedIPs if it exists
            try
            {
                JArray bannedIPsArray = config.BannedIPs;
                if (bannedIPsArray != null)
                    BannedIPs = bannedIPsArray.ToObject<List<string>>();
            }
            catch
            {

            }
        }
        catch (Exception)
        {
            LoggerAccessor.LogWarn("dbmiddleware.json file is malformed, using server's default.");
        }
    }

    public static T GetValueOrDefault<T>(dynamic obj, string propertyName, T defaultValue)
    {
        if (obj != null)
        {
            if (obj is JObject jObject)
            {
                if (jObject.TryGetValue(propertyName, out JToken? value))
                {
                    T? returnvalue = value.ToObject<T>();
                    if (returnvalue != null)
                        return returnvalue;
                }
            }
            else if (obj is JArray jArray)
            {
                if (int.TryParse(propertyName, out int index) && index >= 0 && index < jArray.Count)
                {
                    T? returnvalue = jArray[index].ToObject<T>();
                    if (returnvalue != null)
                        return returnvalue;
                }
            }
        }
        return defaultValue;
    }
}

class Program
{
    private static string configDir = Directory.GetCurrentDirectory() + "/static/";
    private static string configPath = configDir + "dbmiddleware.json";
    private static bool IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows;
    private static HostBuilderServer? Server = null;
    private static Timer? AuthTimer = null;

    private static void StartOrUpdateServer()
    {
        Server?.StopServer();

        SQLiteConnector.StopAllDatabases().Wait();

        AuthenticationChannel.LoadExistingAuths();

        AuthTimer?.Dispose();
        AuthTimer = new Timer(AuthenticationChannel.ScheduledUpdate, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

        SQLiteConnector.AddDatabases(DatabaseMiddlewareServerConfiguration.DbFiles).Wait();

        _ = Task.Run(SQLiteConnector.StartAllDatabases);

        Server = new HostBuilderServer("*", 10000);
    }

    static void Main()
    {
        if (!IsWindows)
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("DatabaseMiddleware", Directory.GetCurrentDirectory());

        GeoIP.Initialize();

        DatabaseMiddlewareServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/dbmiddleware.json");

        StartOrUpdateServer();

        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
        {
            while (true)
            {
                LoggerAccessor.LogInfo("Press any keys to access server actions...");

                Console.ReadLine();

                LoggerAccessor.LogInfo("Press one of the following keys to trigger an action: [R (Reboot),S (Shutdown)]");

                switch (char.ToLower(Console.ReadKey().KeyChar))
                {
                    case 's':
                        LoggerAccessor.LogWarn("Are you sure you want to shut down the server? [y/N]");

                        if (char.ToLower(Console.ReadKey().KeyChar) == 'y')
                        {
                            LoggerAccessor.LogInfo("Shutting down. Goodbye!");
                            Environment.Exit(0);
                        }
                        break;
                    case 'r':
                        LoggerAccessor.LogWarn("Are you sure you want to reboot the server? [y/N]");

                        if (char.ToLower(Console.ReadKey().KeyChar) == 'y')
                        {
                            LoggerAccessor.LogInfo("Rebooting!");

                            DatabaseMiddlewareServerConfiguration.RefreshVariables(configPath);

                            StartOrUpdateServer();
                        }
                        break;
                }
            }
        }
        else
        {
            LoggerAccessor.LogWarn("\nConsole Inputs are locked while server is running. . .");

            Thread.Sleep(Timeout.Infinite);
        }
    }
}