using HTTPServer;
using CustomLogger;
using Newtonsoft.Json.Linq;
using System.Runtime;
using CyberBackendLibrary.GeoLocalization;
using System.IO;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Threading;
using CyberBackendLibrary.AIModels;
using CyberBackendLibrary.HTTP.PluginManager;

public static class HTTPServerConfiguration
{
    public static string PluginsFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/plugins";
    public static ushort DefaultPluginsPort { get; set; } = 61850;
    public static string PHPVersion { get; set; } = "php-8.3.0";
    public static string PHPStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/PHP";
    public static string PHPRedirectUrl { get; set; } = string.Empty;
    public static bool PHPDebugErrors { get; set; } = false;
    public static int BufferSize { get; set; } = 4096;
    public static string HttpVersion { get; set; } = "1.1";
    public static string APIStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwapiroot";
    public static string HTTPStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwroot";
    public static string HTTPTempFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwtemp";
    public static string ConvertersFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/converters";
    public static bool NotFoundSuggestions { get; set; } = false;
    public static bool EnablePUTMethod { get; set; } = false;
    public static bool EnableImageUpscale { get; set; } = false;
    public static Dictionary<string, int>? DateTimeOffset { get; set; }
    public static List<ushort>? Ports { get; set; } = new() { 80, 3074, 9090, 10010, 33000 };
    public static List<string>? RedirectRules { get; set; }
    public static List<string>? BannedIPs { get; set; }

    public static List<HTTPPlugin> plugins = PluginLoader.LoadPluginsFromFolder(PluginsFolder);

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
            LoggerAccessor.LogWarn("Could not find the http.json file, writing and using server's default.");

            Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? Directory.GetCurrentDirectory() + "/static");

            // Write the JObject to a file
            File.WriteAllText(configPath, new JObject(
                new JProperty("php", new JObject(
                    new JProperty("redirect_url", PHPRedirectUrl),
                    new JProperty("version", PHPVersion),
                    new JProperty("static_folder", PHPStaticFolder),
                    new JProperty("debug_errors", PHPDebugErrors)
                )),
                new JProperty("api_static_folder", APIStaticFolder),
                new JProperty("http_static_folder", HTTPStaticFolder),
                new JProperty("http_temp_folder", HTTPTempFolder),
                new JProperty("converters_folder", ConvertersFolder),
                new JProperty("buffer_size", BufferSize),
                new JProperty("http_version", HttpVersion),
                SerializeDateTimeOffset(),
                new JProperty("default_plugins_port", DefaultPluginsPort),
                new JProperty("plugins_folder", PluginsFolder),
                new JProperty("404_not_found_suggestions", NotFoundSuggestions),
                new JProperty("enable_put_method", EnablePUTMethod),
                new JProperty("enable_image_upscale", EnableImageUpscale),
                new JProperty("Ports", new JArray(Ports ?? new List<ushort> { })),
                new JProperty("RedirectRules", new JArray(RedirectRules ?? new List<string> { })),
                new JProperty("BannedIPs", new JArray(BannedIPs ?? new List<string> { }))
            ).ToString().Replace("/", "\\\\"));

            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

            PHPRedirectUrl = GetValueOrDefault(config.php, "redirect_url", PHPRedirectUrl);
            PHPVersion = GetValueOrDefault(config.php, "version", PHPVersion);
            PHPStaticFolder = GetValueOrDefault(config.php, "static_folder", PHPStaticFolder);
            PHPDebugErrors = GetValueOrDefault(config.php, "debug_errors", PHPDebugErrors);
            APIStaticFolder = GetValueOrDefault(config, "api_static_folder", APIStaticFolder);
            HTTPStaticFolder = GetValueOrDefault(config, "http_static_folder", HTTPStaticFolder);
            HTTPTempFolder = GetValueOrDefault(config, "http_temp_folder", HTTPTempFolder);
            ConvertersFolder = GetValueOrDefault(config, "converters_folder", ConvertersFolder);
            BufferSize = GetValueOrDefault(config, "buffer_size", BufferSize);
            HttpVersion = GetValueOrDefault(config, "http_version", HttpVersion);
            DateTimeOffset = GetValueOrDefault(config, "datetime_offset", DateTimeOffset);
            PluginsFolder = GetValueOrDefault(config, "plugins_folder", PluginsFolder);
            DefaultPluginsPort = GetValueOrDefault(config, "default_plugins_port", DefaultPluginsPort);
            NotFoundSuggestions = GetValueOrDefault(config, "404_not_found_suggestions", NotFoundSuggestions);
            EnablePUTMethod = GetValueOrDefault(config, "enable_put_method", EnablePUTMethod);
            EnableImageUpscale = GetValueOrDefault(config, "enable_image_upscale", EnableImageUpscale);
            // Deserialize Ports if it exists
            try
            {
                JArray PortsArray = config.Ports;
                // Deserialize Ports if it exists
                if (PortsArray != null)
                    Ports = PortsArray.ToObject<List<ushort>>();
            }
            catch
            {

            }
            // Deserialize RedirectRules if it exists
            try
            {
                JArray redirectRulesArray = config.RedirectRules;
                // Deserialize RedirectRules if it exists
                if (redirectRulesArray != null)
                    RedirectRules = redirectRulesArray.ToObject<List<string>>();
            }
            catch
            {

            }
            // Deserialize BannedIPs if it exists
            try
            {
                JArray bannedIPsArray = config.BannedIPs;
                // Deserialize BannedIPs if it exists
                if (bannedIPsArray != null)
                    BannedIPs = bannedIPsArray.ToObject<List<string>>();
            }
            catch
            {

            }
        }
        catch (Exception ex)
        {
            LoggerAccessor.LogWarn($"http.json file is malformed (exception: {ex}), using server's default.");
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

    // Helper method for the DateTimeOffset config serialization.
    public static JProperty SerializeDateTimeOffset()
    {
        JObject jObject = new();
        foreach (var kvp in DateTimeOffset ?? new Dictionary<string, int>())
        {
            jObject.Add(kvp.Key, kvp.Value);
        }
        return new JProperty("datetime_offset", jObject);
    }
}

class Program
{
    static Task RefreshConfig()
    {
        while (true)
        {
            // Sleep for 5 minutes (300,000 milliseconds)
            Thread.Sleep(5 * 60 * 1000);

            // Your task logic goes here
            LoggerAccessor.LogInfo("Config Refresh at - " + DateTime.Now);

            HTTPServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/http.json");
        }
    }

    static void Main()
    {
        bool IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows;

        if (!IsWindows)
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("HTTPServer");

        HTTPServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/http.json");

        GeoIP.Initialize();

        if (HTTPServerConfiguration.NotFoundSuggestions)
            _ = new Timer(WebMachineLearning.ScheduledfileSystemUpdate, HTTPServerConfiguration.HTTPStaticFolder, TimeSpan.Zero, TimeSpan.FromMinutes(1440));

        _ = Task.Run(() => Parallel.Invoke(
                    () => _ = new HttpServer(HTTPServerConfiguration.Ports, HTTPServer.RouteHandlers.staticRoutes.Main.index, new CancellationTokenSource().Token),
                    () => RefreshConfig()
                ));

        if (HTTPServerConfiguration.plugins.Count > 0)
        {
            int i = 0;
            foreach (HTTPPlugin plugin in HTTPServerConfiguration.plugins)
            {
                _ = plugin.HTTPStartPlugin(HTTPServerConfiguration.APIStaticFolder, (ushort)(HTTPServerConfiguration.DefaultPluginsPort + i));
                i++;
            }
        }

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