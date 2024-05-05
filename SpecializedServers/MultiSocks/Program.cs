using CustomLogger;
using MultiSocks.DirtySocks;
using Newtonsoft.Json.Linq;
using System.Runtime;
using System.Security.Cryptography;

public static class MultiSocksServerConfiguration
{
    public static bool UsePublicIPAddress { get; set; } = false;
    public static string DirtySocksDatabasePath { get; set; } = $"{Directory.GetCurrentDirectory()}/static/dirtysocks.db.json";

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
            LoggerAccessor.LogWarn("Could not find the MultiSocks.json file, writing and using server's default.");

            Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? Directory.GetCurrentDirectory() + "/static");

            // Write the JObject to a file
            File.WriteAllText(configPath, new JObject(
                new JProperty("use_public_ipaddress", UsePublicIPAddress),
                new JProperty("dirtysocks_database_path", DirtySocksDatabasePath)
            ).ToString().Replace("/", "\\\\"));

            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

            UsePublicIPAddress = GetValueOrDefault(config, "use_public_ipaddress", UsePublicIPAddress);
            DirtySocksDatabasePath = GetValueOrDefault(config, "dirtysocks_database_path", DirtySocksDatabasePath);
        }
        catch (Exception ex)
        {
            LoggerAccessor.LogWarn($"MultiSocks.json file is malformed (exception: {ex}), using server's default.");
        }
    }

    // Helper method to get a value or default value if not present
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
    static string configDir = Directory.GetCurrentDirectory() + "/static/";
    static string configPath = configDir + "MultiSocks.json";
    static bool IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows;
    static DirtySocksServer? DSServer;

    static void StartOrUpdateServer()
    {
        DSServer?.Dispose();
        DSServer = new DirtySocksServer(new CancellationTokenSource().Token);
    }

    static string ComputeMD5FromFile(string filePath)
    {
        using (FileStream stream = File.OpenRead(filePath))
        {
            // Convert the byte array to a hexadecimal string
            return BitConverter.ToString(MD5.Create().ComputeHash(stream)).Replace("-", string.Empty);
        }
    }

    static void Main()
    {
        if (!IsWindows)
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("MultiSocks");

        MultiSocksServerConfiguration.RefreshVariables(configPath);

        StartOrUpdateServer();

        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
        {
            LoggerAccessor.LogInfo("Console Inputs are now available while server is running. . .");

            while (true)
            {
                string? stdin = Console.ReadLine();

                if (!string.IsNullOrEmpty(stdin))
                {
                    switch (stdin.ToLower())
                    {
                        case "shutdown":
                            LoggerAccessor.LogWarn("Are you sure you want to shut down the server? [y/N]");

                            if (char.ToLower(Console.ReadKey().KeyChar) == 'y')
                            {
                                LoggerAccessor.LogInfo("Shutting down. Goodbye!");
                                Environment.Exit(0);
                            }
                            break;
                        case "reboot":
                            LoggerAccessor.LogWarn("Are you sure you want to reboot the server? [y/N]");

                            if (char.ToLower(Console.ReadKey().KeyChar) == 'y')
                            {
                                LoggerAccessor.LogInfo("Rebooting!");

                                MultiSocksServerConfiguration.RefreshVariables(configPath);

                                StartOrUpdateServer();
                            }
                            break;
                        default:
                            LoggerAccessor.LogWarn($"Unknown command entered: {stdin}");
                            break;
                    }
                }
                else
                    LoggerAccessor.LogWarn("No command entered!");
            }
        }
        else
        {
            LoggerAccessor.LogWarn("\nConsole Inputs are locked while server is running. . .");

            Thread.Sleep(Timeout.Infinite);
        }
    }
}