using CustomLogger;
using Newtonsoft.Json.Linq;
using HTTPSecureServerLite;
using System.Runtime;
using WebAPIService.LeaderboardsService;
using CyberBackendLibrary.GeoLocalization;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Threading.Tasks;
using CyberBackendLibrary.AIModels;
using System.Security.Cryptography;
using CyberBackendLibrary.HTTP.PluginManager;
using System.Diagnostics;
using System.Security.Principal;
using HttpMultipartParser;
using SevenZip.Compression.LZ;

public static class HTTPSServerConfiguration
{
    public static string PluginsFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/plugins";
    public static ushort DefaultPluginsPort { get; set; } = 60850;
    public static bool DNSOverEthernetEnabled { get; set; } = true;
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
    public static string HTTPSCertificatePassword { get; set; } = "qwerty";
    public static HashAlgorithmName HTTPSCertificateHashingAlgorithm { get; set; } = HashAlgorithmName.SHA384;
    public static bool NotFoundSuggestions { get; set; } = false;
    public static bool EnablePUTMethod { get; set; } = false;
    public static bool EnableLiveTranscoding { get; set; } = false;
    public static Dictionary<string, int>? DateTimeOffset { get; set; }
    public static string[]? HTTPSDNSList { get; set; } = {
            "www.outso-srv1.com",
            "www.ndreamshs.com",
            "www.development.scee.net",
            "sonyhome.thqsandbox.com",
            "juggernaut-games.com",
            "away.veemee.com",
            "home.veemee.com",
            "pshome.ndreams.net",
            "stats.outso-srv1.com",
            "s3.amazonaws.com",
            "game2.hellfiregames.com",
            "youtube.com",
            "api.pottermore.com",
            "api.stathat.com",
            "hubps3.online.scee.com",
            "homeps3-content.online.scee.com",
            "homeps3.online.scee.com",
            "scee-home.playstation.net",
            "scea-home.playstation.net",
            "update-prod.pfs.online.scee.com",
            "collector.gr.online.scea.com",
            "content.gr.online.scea.com",
            "mmgproject0001.com",
            "massmedia.com",
            "alpha.lootgear.com",
            "server.lootgear.com",
            "prd.destinations.scea.com",
            "root.pshomecasino.com",
            "homeec.scej-nbs.jp",
            "homeecqa.scej-nbs.jp",
            "test.playstationhome.jp",
            "playstationhome.jp",
            "download-prod.online.scea.com",
            "us.ads.playstation.net",
            "ww-prod-sec.destinations.scea.com",
            "ll-100.ea.com",
            "services.heavyh2o.net",
            "secure.cprod.homeps3.online.scee.com",
            "destinationhome.live",
            "prod.homemq.online.scee.com",
            "homeec.scej-nbs.jp",
            "qa-homect-scej.jp",
            "gp1.wac.edgecastcdn.net",
            "api.singstar.online.scee.com",
            "pixeljunk.jp",
            "wpc.33F8.edgecastcdn.net",
            "moonbase.game.co.uk",
            "community.eu.playstation.com",
            "img.game.co.uk",
            "downloads.game.net",
            "example.com",
            "thebissos.com",
            "public-ubiservices.ubi.com",
            "secure.cdevb.homeps3.online.scee.com",
            "www.konami.com",
            "www.ndreamsportal.com",
            "nonprod3.homerewards.online.scee.com",
            "www.services.heavyh2o.net",
            "nDreams-multiserver-cdn"
        };
    public static List<ushort>? Ports { get; set; } = new() { 443 };
    public static List<string>? RedirectRules { get; set; }
    public static List<string>? BannedIPs { get; set; }
    public static Dictionary<int, Dictionary<string, object>>? PluginsCustomParameters { get; set; }

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
            LoggerAccessor.LogWarn("Could not find the https.json file, writing and using server's default.");

            Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? Directory.GetCurrentDirectory() + "/static");

            // Write the JObject to a file
            File.WriteAllText(configPath, new JObject(
                new JProperty("doh_enabled", DNSOverEthernetEnabled),
                new JProperty("online_routes_config", DNSOnlineConfig),
                new JProperty("routes_config", DNSConfig),
                new JProperty("allow_unsafe_requests", DNSAllowUnsafeRequests),
                new JProperty("php", new JObject(
                    new JProperty("redirect_url", PHPRedirectUrl),
                    new JProperty("version", PHPVersion),
                    new JProperty("static_folder", PHPStaticFolder),
                    new JProperty("debug_errors", PHPDebugErrors)
                )),
                new JProperty("api_static_folder", APIStaticFolder),
                new JProperty("https_static_folder", HTTPSStaticFolder),
                new JProperty("https_temp_folder", HTTPSTempFolder),
                SerializeDateTimeOffset(),
                new JProperty("https_dns_list", HTTPSDNSList ?? Array.Empty<string>()),
                new JProperty("converters_folder", ConvertersFolder),
                new JProperty("certificate_file", HTTPSCertificateFile),
                new JProperty("certificate_password", HTTPSCertificatePassword),
                new JProperty("certificate_hashing_algorithm", HTTPSCertificateHashingAlgorithm.Name),
                new JProperty("default_plugins_port", DefaultPluginsPort),
                new JProperty("plugins_folder", PluginsFolder),
                new JProperty("404_not_found_suggestions", NotFoundSuggestions),
                new JProperty("enable_put_method", EnablePUTMethod),
                new JProperty("enable_live_transcoding", EnableLiveTranscoding),
                new JProperty("Ports", new JArray(Ports ?? new List<ushort> { })),
                new JProperty("RedirectRules", new JArray(RedirectRules ?? new List<string> { })),
                new JProperty("BannedIPs", new JArray(BannedIPs ?? new List<string> { })),
                new JProperty("plugins_custom_parameters", string.Empty)
            ).ToString().Replace("/", "\\\\"));

            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

            DNSOverEthernetEnabled = GetValueOrDefault(config, "doh_enabled", DNSOverEthernetEnabled);
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
            HTTPSCertificatePassword = GetValueOrDefault(config, "certificate_password", HTTPSCertificatePassword);
            HTTPSCertificateHashingAlgorithm = new HashAlgorithmName(GetValueOrDefault(config, "certificate_hashing_algorithm", HTTPSCertificateHashingAlgorithm.Name));
            PluginsFolder = GetValueOrDefault(config, "plugins_folder", PluginsFolder);
            DefaultPluginsPort = GetValueOrDefault(config, "default_plugins_port", DefaultPluginsPort);
            NotFoundSuggestions = GetValueOrDefault(config, "404_not_found_suggestions", NotFoundSuggestions);
            EnablePUTMethod = GetValueOrDefault(config, "enable_put_method", EnablePUTMethod);
            EnableLiveTranscoding = GetValueOrDefault(config, "enable_live_transcoding", EnableLiveTranscoding);
            DateTimeOffset = GetValueOrDefault(config, "datetime_offset", DateTimeOffset);
            HTTPSDNSList = GetValueOrDefault(config, "https_dns_list", HTTPSDNSList);
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
            try
            {
                string? NestedJson = config.plugins_custom_parameters;

                if (!string.IsNullOrEmpty(NestedJson))
                {
                    PluginsCustomParameters = ParseNestedJsonArray(NestedJson);

#if DEBUG
                    if (PluginsCustomParameters != null)
                    {
                        foreach (var param in PluginsCustomParameters)
                        {
                            foreach (var keypair in param.Value)
                            {
                                LoggerAccessor.LogInfo($"[HTTPS] - Custom Parameter [{param.Key}] added: {keypair.Key} - {keypair.Value}");
                            }
                        }
                    }
#endif
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogWarn($"Plugins extra data thrown an exception ({ex})");
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

    // Helper method for the DateTimeOffset config serialization.
    private static JProperty SerializeDateTimeOffset()
    {
        JObject jObject = new();
        foreach (var kvp in DateTimeOffset ?? new Dictionary<string, int>())
        {
            jObject.Add(kvp.Key, kvp.Value);
        }
        return new JProperty("datetime_offset", jObject);
    }

    private static Dictionary<int, Dictionary<string, object>> ParseNestedJsonArray(string jsonArray)
    {
        Dictionary<int, Dictionary<string, object>> dictionary = new();

        int i = 0;

        foreach (JObject obj in JArray.Parse(jsonArray).Children<JObject>())
        {
            AddPluginProperties(obj, dictionary, i);
            i++;
        }

        return dictionary;
    }

    private static void AddPluginProperties(JObject obj, Dictionary<int, Dictionary<string, object>> dictionary, int index)
    {
        if (!dictionary.ContainsKey(index))
            dictionary.Add(index, new Dictionary<string, object> { });

        foreach (JProperty property in obj.Properties())
        {
            if (property.Value is JValue value)
                // If value is JValue (primitive), add it directly to dictionary
                dictionary[index][property.Name] = value.Value;
            else if (property.Value is JArray array)
            {
                // If value is JArray, recursively process its elements
                List<object> list = new();
                foreach (var item in array.Children())
                {
                    if (item is JObject @object)
                    {
                        Dictionary<int, Dictionary<string, object>> subDictionary = new();
                        AddPluginProperties(@object, subDictionary, index);
                        list.Add(subDictionary);
                    }
                    else
                        list.Add(((JValue)item).Value);
                }
                dictionary[index][property.Name] = list;
            }
            else if (property.Value is JObject @object)
            {
                // If value is JObject, recursively process its properties
                Dictionary<int, Dictionary<string, object>> subDictionary = new();
                AddPluginProperties(@object, subDictionary, index);
                dictionary[index][property.Name] = subDictionary;
            }
        }
    }
}

class Program
{
    static string configDir = Directory.GetCurrentDirectory() + "/static/";
    static string configPath = configDir + "https.json";
    static string DNSconfigMD5 = string.Empty;
    static bool IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows;
    static Timer? Leaderboard = null;
    static Timer? FilesystemTree = null;
    static Task? DNSThread = null;
    static Task? DNSRefreshThread = null;
    static HTTPSecureServer? Server;
    static readonly FileSystemWatcher dnswatcher = new();

    // Event handler for DNS change event
    static void OnDNSChanged(object source, FileSystemEventArgs e)
    {
        try
        {
            dnswatcher.EnableRaisingEvents = false;

            LoggerAccessor.LogInfo($"DNS Routes File {e.FullPath} has been changed, Routes Refresh at - {DateTime.Now}");

            // Sleep a little to let file-system time to write the changes to the file.
            Thread.Sleep(6000);

            DNSconfigMD5 = ComputeMD5FromFile(HTTPSServerConfiguration.DNSConfig);

            while (DNSRefreshThread != null)
            {
                LoggerAccessor.LogWarn("[HTTPS_DNS] - Waiting for previous DNS refresh Task to finish...");
                Thread.Sleep(6000);
            }

            DNSRefreshThread = RefreshDNS();
            DNSRefreshThread.Dispose();
            DNSRefreshThread = null;
        }

        finally
        {
            dnswatcher.EnableRaisingEvents = true;
        }
    }

    static void StartOrUpdateServer()
    {
        Server?.Stop();
        Server = null;

        if (HTTPSServerConfiguration.EnableLiveTranscoding && IsWindows)
            if (!IsAdministrator())
            {
                LoggerAccessor.LogWarn("Live transcoding functionality needs administrator rights, trying to restart as admin...");
                if (StartAsAdmin(Process.GetCurrentProcess().MainModule?.FileName))
                    Environment.Exit(0);
                else
                    LoggerAccessor.LogError("Live transcoding functionality will not work due to missing administrator rights!");
            }

        CyberBackendLibrary.SSL.SSLUtils.InitCerts(HTTPSServerConfiguration.HTTPSCertificateFile, HTTPSServerConfiguration.HTTPSCertificatePassword,
            HTTPSServerConfiguration.HTTPSDNSList, HTTPSServerConfiguration.HTTPSCertificateHashingAlgorithm);

        LeaderboardClass.APIPath = HTTPSServerConfiguration.APIStaticFolder;

        Leaderboard ??= new Timer(LeaderboardClass.ScheduledUpdate, null, TimeSpan.Zero, TimeSpan.FromMinutes(1440));

        if (HTTPSServerConfiguration.DNSOverEthernetEnabled)
        {
            dnswatcher.Path = Path.GetDirectoryName(HTTPSServerConfiguration.DNSConfig) ?? configDir;
            dnswatcher.Filter = Path.GetFileName(HTTPSServerConfiguration.DNSConfig);
            dnswatcher.EnableRaisingEvents = true;

            if (File.Exists(HTTPSServerConfiguration.DNSConfig))
            {
                string MD5 = ComputeMD5FromFile(HTTPSServerConfiguration.DNSConfig);

                if (!MD5.Equals(DNSconfigMD5))
                {
                    DNSconfigMD5 = MD5;

                    while (DNSRefreshThread != null)
                    {
                        LoggerAccessor.LogWarn("[HTTPS_DNS] - Waiting for previous DNS refresh Task to finish...");
                        Thread.Sleep(6000);
                    }

                    DNSRefreshThread = RefreshDNS();
                    DNSRefreshThread.Dispose();
                    DNSRefreshThread = null;
                }
            }
        }
        else if (dnswatcher.EnableRaisingEvents)
            dnswatcher.EnableRaisingEvents = false;

        if (HTTPSServerConfiguration.NotFoundSuggestions && FilesystemTree == null)
            FilesystemTree = new Timer(WebMachineLearning.ScheduledfileSystemUpdate, HTTPSServerConfiguration.HTTPSStaticFolder, TimeSpan.Zero, TimeSpan.FromMinutes(1440));
        else if (!HTTPSServerConfiguration.NotFoundSuggestions && FilesystemTree != null)
            _ = FilesystemTree.DisposeAsync();

        if (HTTPSServerConfiguration.plugins.Count > 0)
        {
            int i = 0;
            foreach (HTTPPlugin plugin in HTTPSServerConfiguration.plugins)
            {
                if (HTTPSServerConfiguration.PluginsCustomParameters != null && HTTPSServerConfiguration.PluginsCustomParameters.ContainsKey(i))
                    _ = plugin.HTTPStartPlugin(HTTPSServerConfiguration.APIStaticFolder, (ushort)(HTTPSServerConfiguration.DefaultPluginsPort + i), HTTPSServerConfiguration.PluginsCustomParameters[i]);
                else
                    _ = plugin.HTTPStartPlugin(HTTPSServerConfiguration.APIStaticFolder, (ushort)(HTTPSServerConfiguration.DefaultPluginsPort + i), null);
                i++;
            }
        }

        Server = new HTTPSecureServer(HTTPSServerConfiguration.Ports, new CancellationTokenSource().Token);
    }

    static Task RefreshDNS()
    {
        if (DNSThread != null && !SecureDNSConfigProcessor.Initiated)
        {
            while (!SecureDNSConfigProcessor.Initiated)
            {
                LoggerAccessor.LogWarn("[HTTPS_DNS] - Waiting for previous config assignement Task to finish...");
                Thread.Sleep(6000);
            }
        }

        DNSThread = Task.Run(SecureDNSConfigProcessor.InitDNSSubsystem);

        return Task.CompletedTask;
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
        dnswatcher.NotifyFilter = NotifyFilters.LastWrite;
        dnswatcher.Changed += OnDNSChanged;

        if (!IsWindows)
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("HTTPSecureServer");

        GeoIP.Initialize();

        HTTPSServerConfiguration.RefreshVariables(configPath);

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

                                HTTPSServerConfiguration.RefreshVariables(configPath);

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

    /// <summary>
    /// Know if we are the true administrator of the Windows system.
    /// <para>Savoir si est r�ellement l'administrateur Windows.</para>
    /// </summary>
    /// <returns>A boolean.</returns>
#pragma warning disable
    private static bool IsAdministrator()
    {
        return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }
#pragma warning restore

    private static bool StartAsAdmin(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        try
        {
            new Process()
            {
                StartInfo =
                    {
                        FileName = filePath,
                        UseShellExecute = true,
                        Verb = "runas"
                    }
            }.Start();

            return true;
        }
        catch
        {
            return false;
        }
    }
}