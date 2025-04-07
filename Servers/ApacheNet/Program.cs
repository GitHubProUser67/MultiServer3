using CustomLogger;
using Newtonsoft.Json.Linq;
using ApacheNet;
using ApacheNet.PluginManager;
using System.Runtime;
using NetworkLibrary.GeoLocalization;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Threading.Tasks;
using NetworkLibrary.AIModels;
using System.Security.Cryptography;
using System.Reflection;
using NetworkLibrary.HTTP;
using System.Collections.Concurrent;
using NetworkLibrary.Extension;
using System.Diagnostics;

public static class ApacheNetServerConfiguration
{
    public static string PluginsFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/plugins";
    public static ushort DefaultPluginsPort { get; set; } = 60850;
    public static bool DNSOverEthernetEnabled { get; set; } = false;
    public static string DNSConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/routes.txt";
    public static string DNSOnlineConfig { get; set; } = string.Empty;
    public static bool DNSAllowUnsafeRequests { get; set; } = true;
    public static bool EnableAdguardFiltering { get; set; } = false;
    public static bool EnableDanPollockHosts { get; set; } = false;
    public static bool EnableBuiltInPlugins { get; set; } = true;
    public static bool EnableKeepAlive { get; set; } = false;
    public static string HttpVersion { get; set; } = "1.1";
    public static string APIStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwapiroot";
    public static string HTTPStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwroot";
    public static string HTTPSPutFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwtemp";
    public static string ConvertersFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/converters";
    public static string ASPNETRedirectUrl { get; set; } = string.Empty;
    public static string PHPRedirectUrl { get; set; } = string.Empty;
    public static string PHPVersion { get; set; } = "php-8.3.0";
    public static string PHPStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/PHP";
    public static bool PHPDebugErrors { get; set; } = false;
    public static int BufferSize { get; set; } = 4096;
    public static string HTTPSCertificateFile { get; set; } = $"{Directory.GetCurrentDirectory()}/static/SSL/MultiServer.pfx";
    public static string HTTPSCertificatePassword { get; set; } = "qwerty";
    public static HashAlgorithmName HTTPSCertificateHashingAlgorithm { get; set; } = HashAlgorithmName.SHA384;
    public static bool PreferNativeHttpListenerEngine { get; set; } = false;
    public static bool UseLiteEngine { get; set; } = false;
    public static bool RangeHandling { get; set; } = false;
    public static bool ChunkedTransfers { get; set; } = false;
    public static bool DomainFolder { get; set; } = false;
    public static bool NestedDirectoryReporting { get; set; } = false;
    public static bool NotFoundSuggestions { get; set; } = false;
    public static bool NotFoundWebArchive { get; set; } = false;
    public static int NotFoundWebArchiveDateLimit { get; set; } = 0;
    public static bool EnableHTTPCompression { get; set; } = false;
    public static bool EnablePUTMethod { get; set; } = false;
    public static bool EnableImageUpscale { get; set; } = false;
    public static Dictionary<string, string>? MimeTypes { get; set; } = HTTPProcessor._mimeTypes;
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
            "nDreams-multiserver-cdn",
            "secure.cpreprod.homeps3.online.scee.com",
            "secure.heavyh2o.net"
        };
    public static List<ushort>? Ports { get; set; } = new() { 80, 443, 3074, 3658, 9090, 10010, 26004, 33000 };
    public static List<string>? RedirectRules { get; set; }
    public static List<string>? BannedIPs { get; set; }
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
            LoggerAccessor.LogWarn("Could not find the ApacheNet.json file, writing and using server's default.");

            Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? Directory.GetCurrentDirectory() + "/static");

            // Write the JObject to a file
            File.WriteAllText(configPath, new JObject(
                new JProperty("doh_enabled", DNSOverEthernetEnabled),
                new JProperty("online_routes_config", DNSOnlineConfig),
                new JProperty("routes_config", DNSConfig),
                new JProperty("allow_unsafe_requests", DNSAllowUnsafeRequests),
                new JProperty("enable_adguard_filtering", EnableAdguardFiltering),
                new JProperty("enable_dan_pollock_hosts", EnableDanPollockHosts),
                new JProperty("enable_builtin_plugins", EnableBuiltInPlugins),
                new JProperty("enable_keep_alive", EnableKeepAlive),
                new JProperty("aspnet_redirect_url", ASPNETRedirectUrl),
                new JProperty("php", new JObject(
                    new JProperty("redirect_url", PHPRedirectUrl),
                    new JProperty("version", PHPVersion),
                    new JProperty("static_folder", PHPStaticFolder),
                    new JProperty("debug_errors", PHPDebugErrors)
                )),
                new JProperty("api_static_folder", APIStaticFolder),
                new JProperty("https_static_folder", HTTPStaticFolder),
                new JProperty("https_put_folder", HTTPSPutFolder),
                new JProperty("http_version", HttpVersion),
                SerializeMimeTypes(),
                SerializeDateTimeOffset(),
                new JProperty("https_dns_list", HTTPSDNSList ?? Array.Empty<string>()),
                new JProperty("converters_folder", ConvertersFolder),
                new JProperty("buffer_size", BufferSize),
                new JProperty("certificate_file", HTTPSCertificateFile),
                new JProperty("certificate_password", HTTPSCertificatePassword),
                new JProperty("certificate_hashing_algorithm", HTTPSCertificateHashingAlgorithm.Name),
                new JProperty("default_plugins_port", DefaultPluginsPort),
                new JProperty("plugins_folder", PluginsFolder),
                new JProperty("nested_directory_reporting", NestedDirectoryReporting),
                new JProperty("404_not_found_suggestions", NotFoundSuggestions),
                new JProperty("404_not_found_web_archive", NotFoundWebArchive),
                new JProperty("404_not_found_web_archive_date_limit", NotFoundWebArchiveDateLimit),
                new JProperty("prefer_native_httplistener_engine", PreferNativeHttpListenerEngine),
                new JProperty("use_lite_engine", UseLiteEngine),
                new JProperty("enable_range_handling", RangeHandling),
                new JProperty("enable_chunked_transfers", ChunkedTransfers),
                new JProperty("enable_domain_folder", DomainFolder),
                new JProperty("enable_http_compression", EnableHTTPCompression),
                new JProperty("enable_put_method", EnablePUTMethod),
                new JProperty("enable_image_upscale", EnableImageUpscale),
                new JProperty("Ports", new JArray(Ports ?? new List<ushort> { })),
                new JProperty("RedirectRules", new JArray(RedirectRules ?? new List<string> { })),
                new JProperty("BannedIPs", new JArray(BannedIPs ?? new List<string> { })),
                new JProperty("plugins_custom_parameters", string.Empty)
            ).ToString());

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
            EnableAdguardFiltering = GetValueOrDefault(config, "enable_adguard_filtering", EnableAdguardFiltering);
            EnableDanPollockHosts = GetValueOrDefault(config, "enable_dan_pollock_hosts", EnableDanPollockHosts);
            EnableBuiltInPlugins = GetValueOrDefault(config, "enable_builtin_plugins", EnableBuiltInPlugins);
            EnableKeepAlive = GetValueOrDefault(config, "enable_keep_alive", EnableKeepAlive);
            APIStaticFolder = GetValueOrDefault(config, "api_static_folder", APIStaticFolder);
            ASPNETRedirectUrl = GetValueOrDefault(config, "aspnet_redirect_url", ASPNETRedirectUrl);
            PHPRedirectUrl = GetValueOrDefault(config.php, "redirect_url", PHPRedirectUrl);
            PHPVersion = GetValueOrDefault(config.php, "version", PHPVersion);
            PHPStaticFolder = GetValueOrDefault(config.php, "static_folder", PHPStaticFolder);
            PHPDebugErrors = GetValueOrDefault(config.php, "debug_errors", PHPDebugErrors);
            HTTPStaticFolder = GetValueOrDefault(config, "https_static_folder", HTTPStaticFolder);
            HTTPSPutFolder = GetValueOrDefault(config, "https_put_folder", HTTPSPutFolder);
            BufferSize = GetValueOrDefault(config, "buffer_size", BufferSize);
            HttpVersion = GetValueOrDefault(config, "http_version", HttpVersion);
            ConvertersFolder = GetValueOrDefault(config, "converters_folder", ConvertersFolder);
            HTTPSCertificateFile = GetValueOrDefault(config, "certificate_file", HTTPSCertificateFile);
            HTTPSCertificatePassword = GetValueOrDefault(config, "certificate_password", HTTPSCertificatePassword);
            HTTPSCertificateHashingAlgorithm = new HashAlgorithmName(GetValueOrDefault(config, "certificate_hashing_algorithm", HTTPSCertificateHashingAlgorithm.Name));
            PluginsFolder = GetValueOrDefault(config, "plugins_folder", PluginsFolder);
            NestedDirectoryReporting = GetValueOrDefault(config, "nested_directory_reporting", NestedDirectoryReporting);
            DefaultPluginsPort = GetValueOrDefault(config, "default_plugins_port", DefaultPluginsPort);
            NotFoundSuggestions = GetValueOrDefault(config, "404_not_found_suggestions", NotFoundSuggestions);
            NotFoundWebArchive = GetValueOrDefault(config, "404_not_found_web_archive", NotFoundWebArchive);
            NotFoundWebArchiveDateLimit = GetValueOrDefault(config, "404_not_found_web_archive_date_limit", NotFoundWebArchiveDateLimit);
            PreferNativeHttpListenerEngine = GetValueOrDefault(config, "prefer_native_httplistener_engine", PreferNativeHttpListenerEngine);
            UseLiteEngine = GetValueOrDefault(config, "use_lite_engine", UseLiteEngine);
            RangeHandling = GetValueOrDefault(config, "enable_range_handling", RangeHandling);
            ChunkedTransfers = GetValueOrDefault(config, "enable_chunked_transfers", ChunkedTransfers);
            DomainFolder = GetValueOrDefault(config, "enable_domain_folder", DomainFolder);
            EnableHTTPCompression = GetValueOrDefault(config, "enable_http_compression", EnableHTTPCompression);
            EnablePUTMethod = GetValueOrDefault(config, "enable_put_method", EnablePUTMethod);
            EnableImageUpscale = GetValueOrDefault(config, "enable_image_upscale", EnableImageUpscale);
            MimeTypes = GetValueOrDefault(config, "mime_types", MimeTypes);
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
        }
        catch (Exception ex)
        {
            LoggerAccessor.LogWarn($"ApacheNet.json file is malformed (exception: {ex}), using server's default.");
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
}

class Program
{
    private static string configDir = Directory.GetCurrentDirectory() + "/static/";
    private static string configPath = configDir + "ApacheNet.json";
    private static string DNSconfigMD5 = string.Empty;
    private static Timer? FilesystemTree = null;
    private static Task? DNSThread = null;
    private static Task? DNSRefreshThread = null;
    private static ConcurrentBag<ApacheNetProcessor>? HTTPSBag = null;
    private static readonly FileSystemWatcher dnswatcher = new();

    // Event handler for DNS change event
    private static void OnDNSChanged(object source, FileSystemEventArgs e)
    {
        try
        {
            dnswatcher.EnableRaisingEvents = false;

            LoggerAccessor.LogInfo($"DNS Routes File {e.FullPath} has been changed, Routes Refresh at - {DateTime.Now}");

            // Sleep a little to let file-system time to write the changes to the file.
            Thread.Sleep(6000);

            DNSconfigMD5 = ComputeMD5FromFile(ApacheNetServerConfiguration.DNSConfig);

            while (DNSRefreshThread != null)
            {
                LoggerAccessor.LogWarn("[ApacheNet] - Waiting for previous DNS refresh Task to finish...");
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

    private static void StartOrUpdateServer()
    {
        if (HTTPSBag != null)
        {
            foreach (ApacheNetProcessor httpsBag in HTTPSBag)
            {
                httpsBag.StopServer();
            }
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        if (ApacheNetServerConfiguration.EnableAdguardFiltering)
            _ = ApacheNetProcessor.adChecker.DownloadAndParseFilterListAsync();
        if (ApacheNetServerConfiguration.EnableDanPollockHosts)
            _ = ApacheNetProcessor.danChecker.DownloadAndParseFilterListAsync();

        WebAPIService.WebArchive.WebArchiveRequest.ArchiveDateLimit = ApacheNetServerConfiguration.NotFoundWebArchiveDateLimit;

        NetworkLibrary.SSL.CertificateHelper.InitializeSSLChainSignedCertificates(ApacheNetServerConfiguration.HTTPSCertificateFile, ApacheNetServerConfiguration.HTTPSCertificatePassword,
            ApacheNetServerConfiguration.HTTPSDNSList, ApacheNetServerConfiguration.HTTPSCertificateHashingAlgorithm);

        if (ApacheNetServerConfiguration.DNSOverEthernetEnabled)
        {
            dnswatcher.Path = Path.GetDirectoryName(ApacheNetServerConfiguration.DNSConfig) ?? configDir;
            dnswatcher.Filter = Path.GetFileName(ApacheNetServerConfiguration.DNSConfig);
            dnswatcher.EnableRaisingEvents = true;

            if (File.Exists(ApacheNetServerConfiguration.DNSConfig))
            {
                string MD5 = ComputeMD5FromFile(ApacheNetServerConfiguration.DNSConfig);

                if (!MD5.Equals(DNSconfigMD5))
                {
                    DNSconfigMD5 = MD5;

                    while (DNSRefreshThread != null)
                    {
                        LoggerAccessor.LogWarn("[ApacheNet] - Waiting for previous DNS refresh Task to finish...");
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

        if (ApacheNetServerConfiguration.NotFoundSuggestions && FilesystemTree == null)
            FilesystemTree = new Timer(WebMachineLearning.ScheduledfileSystemUpdate, ApacheNetServerConfiguration.HTTPStaticFolder, TimeSpan.Zero, TimeSpan.FromMinutes(1440));
        else if (!ApacheNetServerConfiguration.NotFoundSuggestions && FilesystemTree != null)
            _ = FilesystemTree.DisposeAsync();

        if (ApacheNetServerConfiguration.plugins.Count > 0)
        {
            int i = 0;
            foreach (var plugin in ApacheNetServerConfiguration.plugins)
            {
                _ = plugin.Value.HTTPStartPlugin(ApacheNetServerConfiguration.APIStaticFolder, (ushort)(ApacheNetServerConfiguration.DefaultPluginsPort + i));
                i++;
            }
        }

        if (ApacheNetServerConfiguration.Ports != null && ApacheNetServerConfiguration.Ports.Count > 0)
        {
            HTTPSBag = new();

            Parallel.ForEach(ApacheNetServerConfiguration.Ports, port =>
            {
                if (TCPUtils.IsTCPPortAvailable(port))
                    HTTPSBag.Add(new ApacheNetProcessor(ApacheNetServerConfiguration.HTTPSCertificateFile, ApacheNetServerConfiguration.HTTPSCertificatePassword, "*", port, port.ToString().EndsWith("443")));
            });
        }
        else
        {
            HTTPSBag = null;
            LoggerAccessor.LogError("[ApacheNet] - No ports were found in the server configuration, ignoring server startup...");
        }
    }

    private static Task RefreshDNS()
    {
        if (DNSThread != null && !SecureDNSConfigProcessor.Initiated)
        {
            while (!SecureDNSConfigProcessor.Initiated)
            {
                LoggerAccessor.LogWarn("[ApacheNet] - Waiting for previous config assignement Task to finish...");
                Thread.Sleep(6000);
            }
        }

        DNSThread = Task.Run(SecureDNSConfigProcessor.InitDNSSubsystem);

        return Task.CompletedTask;
    }

    private static string ComputeMD5FromFile(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        // Convert the byte array to a hexadecimal string
        return NetHasher.DotNetHasher.ComputeMD5String(stream);
    }

    static void Main()
    {
        dnswatcher.NotifyFilter = NotifyFilters.LastWrite;
        dnswatcher.Changed += OnDNSChanged;

        if (!NetworkLibrary.Extension.Windows.Win32API.IsWindows)
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        else
            TechnitiumLibrary.Net.Firewall.FirewallHelper.CheckFirewallEntries(Assembly.GetEntryAssembly()?.Location);

        LoggerAccessor.SetupLogger("ApacheNet", Directory.GetCurrentDirectory());

#if DEBUG
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            LoggerAccessor.LogError("[Program] - A FATAL ERROR OCCURED!");
            LoggerAccessor.LogError(args.ExceptionObject as Exception);
        };

        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            LoggerAccessor.LogError("[Program] - A task has thrown a Unobserved Exception!");
            LoggerAccessor.LogError(args.Exception);
            args.SetObserved();
        };

        InternetProtocolUtils.GetIPInfos(InternetProtocolUtils.GetLocalIPAddress().ToString(), InternetProtocolUtils.GetLocalSubnet());
#endif

        GeoIP.Initialize();

        ApacheNetServerConfiguration.RefreshVariables(configPath);

        if (ApacheNetServerConfiguration.PreferNativeHttpListenerEngine
            && NetworkLibrary.Extension.Windows.Win32API.IsWindows
            && !NetworkLibrary.Extension.Windows.Win32API.IsAdministrator())
        {
            LoggerAccessor.LogWarn("[Program] - Trying to restart as admin...");
            if (NetworkLibrary.Extension.Windows.Win32API.StartAsAdmin(Process.GetCurrentProcess().MainModule?.FileName))
                Environment.Exit(0);
        }

        ApacheNetProcessor.Routes.AddRange(ApacheNet.RouteHandlers.Main.index);

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

                            ApacheNetServerConfiguration.RefreshVariables(configPath);

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