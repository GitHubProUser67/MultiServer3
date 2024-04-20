
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
    static async Task RefreshConfig()
    {
        while (true)
        {
            // Sleep for 5 minutes (300,000 milliseconds)
            Thread.Sleep(5 * 60 * 1000);

            // Your task logic goes here
            LoggerAccessor.LogInfo("Config Refresh at - " + DateTime.Now);

            DatabaseMiddlewareServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/dbmiddleware.json");

            await SQLiteConnector.AddDatabases(DatabaseMiddlewareServerConfiguration.DbFiles);
            await SQLiteConnector.StartAllDatabases();
        }
    }

    static void Main()
    {
        bool IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows;

        if (!IsWindows)
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("DatabaseMiddleware");

        DatabaseMiddlewareServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/dbmiddleware.json");

        AuthenticationChannel.LoadExistingAuths();

        _ = new Timer(AuthenticationChannel.ScheduledUpdate, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

        GeoIP.Initialize();

        SQLiteConnector.AddDatabases(DatabaseMiddlewareServerConfiguration.DbFiles).Wait();

        _ = Task.Run(() => Parallel.Invoke(
                    () => _ = SQLiteConnector.StartAllDatabases(),
                    () => new HostBuilderServer("*", 10000).StartServer(),
                    () => _ = RefreshConfig()
                ));

        if (IsWindows)
        {
            while (true)
            {
                LoggerAccessor.LogInfo("Press any key to shutdown the server. . .");

                Console.ReadLine();

                LoggerAccessor.LogWarn("Are you sure you want to shut down the server? [y/N]");

                if (char.ToLower(Console.ReadKey().KeyChar) == 'y')
                {
                    LoggerAccessor.LogInfo("Shutting down. Goodbye!");
                    Environment.Exit(0);
                }
            }
        }
        else
        {
            LoggerAccessor.LogWarn("\nConsole Inputs are locked while server is running. . .");

            Thread.Sleep(Timeout.Infinite); // While-true on Linux are thread blocking if on main static.
        }
    }
}