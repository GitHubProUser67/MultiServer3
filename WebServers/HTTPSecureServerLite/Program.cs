using CustomLogger;
using Newtonsoft.Json.Linq;
using HTTPSecureServerLite;
using System.Runtime;
using HomeTools.AFS;
using WebAPIService.LeaderboardsService;
using CyberBackendLibrary.GeoLocalization;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Threading.Tasks;

public static class HTTPSServerConfiguration
{
    public static string PluginsFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/plugins";
    public static ushort DefaultPluginsPort { get; set; } = 60850;
    public static string DNSConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/routes.txt";
    public static string DNSOnlineConfig { get; set; } = string.Empty;
    public static bool DNSAllowUnsafeRequests { get; set; } = true;
    public static string APIStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwapiroot";
    public static string HTTPSStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwroot";
    public static string HTTPSTempFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwtemp";
    public static string ConvertersFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/converters";
    public static string PHPRedirectUrl { get; set; } = string.Empty;
    public static string PHPVersion { get; set; } = "php-8.3.0";
    public static string PHPStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/PHP";
    public static bool PHPDebugErrors { get; set; } = false;
    public static string HTTPSCertificateFile { get; set; } = $"{Directory.GetCurrentDirectory()}/static/SSL/MultiServer.pfx";
    public static bool UseSelfSignedCertificate { get; set; } = false;
    public static bool EnablePUTMethod { get; set; } = false;
    public static List<ushort>? Ports { get; set; } = new() { 443 };
    public static List<string>? RedirectRules { get; set; }
    public static List<string>? BannedIPs { get; set; }

    public static List<HTTPSecureServerLite.PluginManager.HTTPSecurePlugin> plugins = HTTPSecureServerLite.PluginManager.PluginLoader.LoadPluginsFromFolder(PluginsFolder);

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
            LoggerAccessor.LogWarn("Could not find the https.json file, writing and using server's default.");

            Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? Directory.GetCurrentDirectory() + "/static");

            // Write the JObject to a file
            File.WriteAllText(configPath, new JObject(
                new JProperty("online_routes_config", DNSOnlineConfig),
                new JProperty("routes_config", DNSConfig),
                new JProperty("allow_unsafe_requests", DNSAllowUnsafeRequests),
                new JProperty("api_static_folder", APIStaticFolder),
                new JProperty("php", new JObject(
                    new JProperty("redirect_url", PHPRedirectUrl),
                    new JProperty("version", PHPVersion),
                    new JProperty("static_folder", PHPStaticFolder),
                    new JProperty("debug_errors", PHPDebugErrors)
                )),
                new JProperty("https_static_folder", HTTPSStaticFolder),
                new JProperty("https_temp_folder", HTTPSTempFolder),
                new JProperty("converters_folder", ConvertersFolder),
                new JProperty("certificate_file", HTTPSCertificateFile),
                new JProperty("use_self_signed_certificate", UseSelfSignedCertificate),
                new JProperty("default_plugins_port", DefaultPluginsPort),
                new JProperty("plugins_folder", PluginsFolder),
                new JProperty("enable_put_method", EnablePUTMethod),
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

            DNSOnlineConfig = GetValueOrDefault(config, "online_routes_config", DNSOnlineConfig);
            DNSConfig = GetValueOrDefault(config, "routes_config", DNSConfig);
            DNSAllowUnsafeRequests = GetValueOrDefault(config, "allow_unsafe_requests", DNSAllowUnsafeRequests);
            APIStaticFolder = GetValueOrDefault(config, "api_static_folder", APIStaticFolder);
            PHPRedirectUrl = GetValueOrDefault(config.php, "redirect_url", PHPRedirectUrl);
            PHPVersion = GetValueOrDefault(config.php, "version", PHPVersion);
            PHPStaticFolder = GetValueOrDefault(config.php, "static_folder", PHPStaticFolder);
            PHPDebugErrors = GetValueOrDefault(config.php, "debug_errors", PHPDebugErrors);
            HTTPSStaticFolder = GetValueOrDefault(config, "https_static_folder", HTTPSStaticFolder);
            HTTPSTempFolder = GetValueOrDefault(config, "https_temp_folder", HTTPSTempFolder);
            ConvertersFolder = GetValueOrDefault(config, "converters_folder", ConvertersFolder);
            HTTPSCertificateFile = GetValueOrDefault(config, "certificate_file", HTTPSCertificateFile);
            UseSelfSignedCertificate = GetValueOrDefault(config, "use_self_signed_certificate", UseSelfSignedCertificate);
            PluginsFolder = GetValueOrDefault(config, "plugins_folder", PluginsFolder);
            DefaultPluginsPort = GetValueOrDefault(config, "default_plugins_port", DefaultPluginsPort);
            EnablePUTMethod = GetValueOrDefault(config, "enable_put_method", EnablePUTMethod);
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
            LoggerAccessor.LogWarn($"https.json file is malformed (exception: {ex}), using server's default.");
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
    static Task RefreshConfig()
    {
        while (true)
        {
            // Sleep for 5 minutes (300,000 milliseconds)
            Thread.Sleep(5 * 60 * 1000);

            // Your task logic goes here
            LoggerAccessor.LogInfo("Config Refresh at - " + DateTime.Now);

            HTTPSServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/https.json");
        }
    }

    static void Main()
    {
        bool IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows;

        if (!IsWindows)
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("HTTPSecureServer");

        HTTPSServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/https.json");

        string certpath = HTTPSServerConfiguration.HTTPSCertificateFile;

        CyberBackendLibrary.SSL.SSLUtils.InitCerts(certpath);

        GeoIP.Initialize();

        LeaderboardClass.APIPath = HTTPSServerConfiguration.APIStaticFolder;

        _ = new Timer(AFSClass.ScheduledUpdate, null, TimeSpan.Zero, TimeSpan.FromMinutes(1440));
        _ = new Timer(LeaderboardClass.ScheduledUpdate, null, TimeSpan.Zero, TimeSpan.FromMinutes(1440));

        _ = Task.Run(() => Parallel.Invoke(
                    () => SecureDNSConfigProcessor.InitDNSSubsystem(),
                    () => Parallel.ForEach(HTTPSServerConfiguration.Ports ?? new List<ushort> { }, port =>
                    {
                        new HttpsProcessor(HTTPSServerConfiguration.UseSelfSignedCertificate ? Path.GetDirectoryName(certpath) + $"/{Path.GetFileNameWithoutExtension(certpath)}_selfsigned.pfx" : certpath, "qwerty", "0.0.0.0", port).StartServer(); // 0.0.0.0 as the certificate binds to this IP
                    }),
                    () => RefreshConfig()
                ));

        if (HTTPSServerConfiguration.plugins.Count > 0)
        {
            int i = 0;
            foreach (HTTPSecureServerLite.PluginManager.HTTPSecurePlugin plugin in HTTPSServerConfiguration.plugins)
            {
                _ = plugin.HTTPSecureStartPlugin(HTTPSServerConfiguration.APIStaticFolder, (ushort)(HTTPSServerConfiguration.DefaultPluginsPort + i));
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