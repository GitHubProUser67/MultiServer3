using MozaicHTTP;
using CustomLogger;
using Newtonsoft.Json.Linq;
using System.Runtime;
using CyberBackendLibrary.GeoLocalization;
using System.IO;
using System.Collections.Generic;
using System;
using System.Threading;
using CyberBackendLibrary.AIModels;
using CyberBackendLibrary.HTTP.PluginManager;
using System.Reflection;
using CyberBackendLibrary.HTTP;
using System.Security.Cryptography;
using WebAPIService.LeaderboardsService;

public static class MozaicHTTPConfiguration
{
    public static string PluginsFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/plugins";
    public static ushort DefaultPluginsPort { get; set; } = 61850;
    public static string ASPNETRedirectUrl { get; set; } = string.Empty;
    public static string PHPRedirectUrl { get; set; } = string.Empty;
    public static string PHPVersion { get; set; } = "php-8.3.0";
    public static string PHPStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/PHP";
    public static bool PHPDebugErrors { get; set; } = false;
    public static int BufferSize { get; set; } = 4096;
    public static string HTTPSCertificateFile { get; set; } = $"{Directory.GetCurrentDirectory()}/static/SSL/MultiServer.pfx";
    public static string HTTPSCertificatePassword { get; set; } = "qwerty";
    public static HashAlgorithmName HTTPSCertificateHashingAlgorithm { get; set; } = HashAlgorithmName.SHA384;
    public static string HttpVersion { get; set; } = "1.1";
    public static string APIStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwapiroot";
    public static string HTTPStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwroot";
    public static string HTTPTempFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwtemp";
    public static string ConvertersFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/converters";
    public static bool AcceptInvalidSSLCertificates { get; set; } = true;
    public static bool NotFoundSuggestions { get; set; } = false;
    public static bool EnableHTTPChunkedTransfers { get; set; } = false;
    public static bool EnableHTTPCompression { get; set; } = true;
    public static bool EnablePUTMethod { get; set; } = false;
    public static bool EnableImageUpscale { get; set; } = false;
    public static Dictionary<string, string>? MimeTypes { get; set; } = HTTPProcessor._mimeTypes;
    public static Dictionary<string, int>? DateTimeOffset { get; set; }
    public static Dictionary<string, bool>? Ports { get; set; } = new() { { "80", false }, { "3074", false }, { "9090", false }, { "10010", false }, { "33000", false }, { "443", true } };
    public static List<string>? RedirectRules { get; set; }
    public static List<string>? BannedIPs { get; set; }
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

    public static Dictionary<string, HTTPPlugin> plugins = PluginLoader.LoadPluginsFromFolder(PluginsFolder);

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
            LoggerAccessor.LogWarn("Could not find the mozaic.json file, writing and using server's default.");

            Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? Directory.GetCurrentDirectory() + "/static");

            // Write the JObject to a file
            File.WriteAllText(configPath, new JObject(
                new JProperty("aspnet_redirect_url", ASPNETRedirectUrl),
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
                new JProperty("accept_invalid_ssl_certificates", AcceptInvalidSSLCertificates),
                new JProperty("certificate_file", HTTPSCertificateFile),
                new JProperty("certificate_password", HTTPSCertificatePassword),
                new JProperty("certificate_hashing_algorithm", HTTPSCertificateHashingAlgorithm.Name),
                new JProperty("http_version", HttpVersion),
                SerializeMimeTypes(),
                SerializeDateTimeOffset(),
                new JProperty("default_plugins_port", DefaultPluginsPort),
                new JProperty("plugins_folder", PluginsFolder),
                new JProperty("404_not_found_suggestions", NotFoundSuggestions),
                new JProperty("enable_http_chunked_transfers", EnableHTTPChunkedTransfers),
                new JProperty("enable_http_compression", EnableHTTPCompression),
                new JProperty("enable_put_method", EnablePUTMethod),
                new JProperty("enable_image_upscale", EnableImageUpscale),
                SerializePorts(),
                new JProperty("RedirectRules", new JArray(RedirectRules ?? new List<string> { })),
                new JProperty("BannedIPs", new JArray(BannedIPs ?? new List<string> { })),
                new JProperty("https_dns_list", HTTPSDNSList ?? Array.Empty<string>())
            ).ToString());

            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

            ASPNETRedirectUrl = GetValueOrDefault(config, "aspnet_redirect_url", ASPNETRedirectUrl);
            PHPRedirectUrl = GetValueOrDefault(config.php, "redirect_url", PHPRedirectUrl);
            PHPVersion = GetValueOrDefault(config.php, "version", PHPVersion);
            PHPStaticFolder = GetValueOrDefault(config.php, "static_folder", PHPStaticFolder);
            PHPDebugErrors = GetValueOrDefault(config.php, "debug_errors", PHPDebugErrors);
            APIStaticFolder = GetValueOrDefault(config, "api_static_folder", APIStaticFolder);
            HTTPStaticFolder = GetValueOrDefault(config, "http_static_folder", HTTPStaticFolder);
            HTTPTempFolder = GetValueOrDefault(config, "http_temp_folder", HTTPTempFolder);
            ConvertersFolder = GetValueOrDefault(config, "converters_folder", ConvertersFolder);
            BufferSize = GetValueOrDefault(config, "buffer_size", BufferSize);
            AcceptInvalidSSLCertificates = GetValueOrDefault(config, "accept_invalid_ssl_certificates", AcceptInvalidSSLCertificates);
            HTTPSCertificateFile = GetValueOrDefault(config, "certificate_file", HTTPSCertificateFile);
            HTTPSCertificatePassword = GetValueOrDefault(config, "certificate_password", HTTPSCertificatePassword);
            HTTPSCertificateHashingAlgorithm = new HashAlgorithmName(GetValueOrDefault(config, "certificate_hashing_algorithm", HTTPSCertificateHashingAlgorithm.Name));
            HttpVersion = GetValueOrDefault(config, "http_version", HttpVersion);
            MimeTypes = GetValueOrDefault(config, "mime_types", MimeTypes);
            DateTimeOffset = GetValueOrDefault(config, "datetime_offset", DateTimeOffset);
            PluginsFolder = GetValueOrDefault(config, "plugins_folder", PluginsFolder);
            DefaultPluginsPort = GetValueOrDefault(config, "default_plugins_port", DefaultPluginsPort);
            NotFoundSuggestions = GetValueOrDefault(config, "404_not_found_suggestions", NotFoundSuggestions);
            EnableHTTPChunkedTransfers = GetValueOrDefault(config, "enable_http_chunked_transfers", EnableHTTPChunkedTransfers);
            EnableHTTPCompression = GetValueOrDefault(config, "enable_http_compression", EnableHTTPCompression);
            EnablePUTMethod = GetValueOrDefault(config, "enable_put_method", EnablePUTMethod);
            EnableImageUpscale = GetValueOrDefault(config, "enable_image_upscale", EnableImageUpscale);
            HTTPSDNSList = GetValueOrDefault(config, "https_dns_list", HTTPSDNSList);
            Ports = GetValueOrDefault(config, "Ports", Ports);
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
            LoggerAccessor.LogWarn($"mozaic.json file is malformed (exception: {ex}), using server's default.");
        }
    }

    // Helper method to get a value or default value if not present
    public static T GetValueOrDefault<T>(dynamic obj, string propertyName, T defaultValue)
    {
        try
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
            }
        }
        catch (Exception ex)
        {
            LoggerAccessor.LogError($"[Program] - GetValueOrDefault thrown an exception: {ex}");
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

    // Helper method for the MimeTypes config serialization.
    private static JProperty SerializeMimeTypes()
    {
        JObject jObject = new();
        foreach (var kvp in MimeTypes ?? new Dictionary<string, string>())
        {
            jObject.Add(kvp.Key, kvp.Value);
        }
        return new JProperty("mime_types", jObject);
    }

    // Helper method for the Ports config serialization.
    private static JProperty SerializePorts()
    {
        JObject jObject = new();
        foreach (var kvp in Ports ?? new Dictionary<string, bool>())
        {
            jObject.Add(kvp.Key, kvp.Value);
        }
        return new JProperty("Ports", jObject);
    }
}

class Program
{
    private static string configDir = Directory.GetCurrentDirectory() + "/static/";
    private static string configPath = configDir + "mozaic.json";
    private static bool IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows;
    private static Timer? Leaderboard = null;
    private static Timer? FilesystemTree = null;
    private static HttpServer? Server;

    private static void StartOrUpdateServer()
    {
        Server?.Stop();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        CyberBackendLibrary.SSL.SSLUtils.InitCerts(MozaicHTTPConfiguration.HTTPSCertificateFile, MozaicHTTPConfiguration.HTTPSCertificatePassword,
            MozaicHTTPConfiguration.HTTPSDNSList, MozaicHTTPConfiguration.HTTPSCertificateHashingAlgorithm);

        if (MozaicHTTPConfiguration.NotFoundSuggestions && FilesystemTree == null)
            FilesystemTree = new Timer(WebMachineLearning.ScheduledfileSystemUpdate, MozaicHTTPConfiguration.HTTPStaticFolder, TimeSpan.Zero, TimeSpan.FromMinutes(1440));
        else if (!MozaicHTTPConfiguration.NotFoundSuggestions && FilesystemTree != null)
        {
            FilesystemTree.Dispose();
            FilesystemTree = null;
        }

        LeaderboardClass.APIPath = MozaicHTTPConfiguration.APIStaticFolder;

        Leaderboard ??= new Timer(LeaderboardClass.ScheduledUpdate, null, TimeSpan.Zero, TimeSpan.FromMinutes(1440));

        if (MozaicHTTPConfiguration.plugins.Count > 0)
        {
            int i = 0;
            foreach (var plugin in MozaicHTTPConfiguration.plugins)
            {
                _ = plugin.Value.HTTPStartPlugin(MozaicHTTPConfiguration.APIStaticFolder, (ushort)(MozaicHTTPConfiguration.DefaultPluginsPort + i));
                i++;
            }
        }

        Server = new HttpServer(MozaicHTTPConfiguration.Ports, MozaicHTTP.RouteHandlers.staticRoutes.Main.index, new CancellationTokenSource().Token);
    }

    static void Main()
    {
        if (!IsWindows)
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        else
            TechnitiumLibrary.Net.Firewall.FirewallHelper.CheckFirewallEntries(Assembly.GetEntryAssembly()?.Location);

        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;

        LoggerAccessor.SetupLogger("MozaicHTTP", Directory.GetCurrentDirectory());

        GeoIP.Initialize();

        MozaicHTTPConfiguration.RefreshVariables(configPath);

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

                            MozaicHTTPConfiguration.RefreshVariables(configPath);

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