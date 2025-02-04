using HTTPServer;
using CustomLogger;
using Newtonsoft.Json.Linq;
using System.Runtime;
using NetworkLibrary.GeoLocalization;
using System.IO;
using System.Collections.Generic;
using System;
using System.Threading;
using NetworkLibrary.AIModels;
using NetworkLibrary.HTTP.PluginManager;
using System.Reflection;
using NetworkLibrary.HTTP;
using System.Threading.Tasks;
using NetworkLibrary.TCP_IP;
using System.Collections.Concurrent;
using System.Linq;

public static class HTTPServerConfiguration
{
    public static string PluginsFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/plugins";
    public static ushort DefaultPluginsPort { get; set; } = 61850;
    public static string ASPNETRedirectUrl { get; set; } = string.Empty;
    public static string PHPRedirectUrl { get; set; } = string.Empty;
    public static string PHPVersion { get; set; } = "php-8.3.0";
    public static string PHPStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/PHP";
    public static bool PHPDebugErrors { get; set; } = false;
    public static int BufferSize { get; set; } = 4096;
    public static int MaximumAllowedKeepAliveClients { get; set; } = 150;
    public static string HttpVersion { get; set; } = "1.1";
    public static string APIStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwapiroot";
    public static string HTTPStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwroot";
    public static string HTTPTempFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwtemp";
    public static string ConvertersFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/converters";
    public static bool ChunkedTransfers { get; set; } = false;
    public static bool DomainFolder { get; set; } = false;
    public static bool NotFoundSuggestions { get; set; } = false;
    public static bool NotFoundWebArchive { get; set; } = false;
    public static int NotFoundWebArchiveDateLimit { get; set; } = 0;
    public static bool EnableHTTPCompression { get; set; } = true;
    public static bool EnablePUTMethod { get; set; } = false;
    public static bool EnableImageUpscale { get; set; } = false;
    public static Dictionary<string, string>? MimeTypes { get; set; } = HTTPProcessor._mimeTypes;
    public static Dictionary<string, int>? DateTimeOffset { get; set; }
    public static List<ushort>? Ports { get; set; } = new() { 80, 3074, 3658, 9090, 10010, 33000 };
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
            LoggerAccessor.LogWarn("Could not find the http.json file, writing and using server's default.");

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
                new JProperty("maximum_allowed_keep_alive_clients", MaximumAllowedKeepAliveClients),
                new JProperty("http_version", HttpVersion),
                SerializeMimeTypes(),
                SerializeDateTimeOffset(),
                new JProperty("default_plugins_port", DefaultPluginsPort),
                new JProperty("plugins_folder", PluginsFolder),
                new JProperty("404_not_found_suggestions", NotFoundSuggestions),
                new JProperty("404_not_found_web_archive", NotFoundWebArchive),
                new JProperty("404_not_found_web_archive_date_limit", NotFoundWebArchiveDateLimit),
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
            MaximumAllowedKeepAliveClients = GetValueOrDefault(config, "maximum_allowed_keep_alive_clients", MaximumAllowedKeepAliveClients);
            HttpVersion = GetValueOrDefault(config, "http_version", HttpVersion);
            MimeTypes = GetValueOrDefault(config, "mime_types", MimeTypes);
            DateTimeOffset = GetValueOrDefault(config, "datetime_offset", DateTimeOffset);
            PluginsFolder = GetValueOrDefault(config, "plugins_folder", PluginsFolder);
            DefaultPluginsPort = GetValueOrDefault(config, "default_plugins_port", DefaultPluginsPort);
            NotFoundSuggestions = GetValueOrDefault(config, "404_not_found_suggestions", NotFoundSuggestions);
            NotFoundWebArchive = GetValueOrDefault(config, "404_not_found_web_archive", NotFoundWebArchive);
            NotFoundWebArchiveDateLimit = GetValueOrDefault(config, "404_not_found_web_archive_date_limit", NotFoundWebArchiveDateLimit);
            ChunkedTransfers = GetValueOrDefault(config, "enable_chunked_transfers", ChunkedTransfers);
            DomainFolder = GetValueOrDefault(config, "enable_domain_folder", DomainFolder);
            EnableHTTPCompression = GetValueOrDefault(config, "enable_http_compression", EnableHTTPCompression);
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
    private static string configPath = configDir + "http.json";
    private static Timer? FilesystemTree = null;
    private static ConcurrentDictionary<ushort, HTTPServer.HTTPServer>? HTTPBag = null;
    private static readonly HttpProcessor Processor = InitializeProcessor();

    private static HttpProcessor InitializeProcessor()
    {
        HttpProcessor proc = new();

        HTTPServer.RouteHandlers.staticRoutes.Main.index.ForEach(route => proc.AddRoute(route));

        return proc;
    }

    private static void StartOrUpdateServer()
    {
        if (HTTPBag != null)
        {
            foreach (var httpBag in HTTPBag.Values)
            {
                httpBag.ExitSignal = true;
            }
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        WebAPIService.WebArchive.WebArchiveRequest.ArchiveDateLimit = HTTPServerConfiguration.NotFoundWebArchiveDateLimit;

        if (HTTPServerConfiguration.NotFoundSuggestions && FilesystemTree == null)
            FilesystemTree = new Timer(WebMachineLearning.ScheduledfileSystemUpdate, HTTPServerConfiguration.HTTPStaticFolder, TimeSpan.Zero, TimeSpan.FromMinutes(1440));
        else if (!HTTPServerConfiguration.NotFoundSuggestions && FilesystemTree != null)
            _ = FilesystemTree.DisposeAsync();

        if (HTTPServerConfiguration.plugins.Count > 0)
        {
            int i = 0;
            foreach (var plugin in HTTPServerConfiguration.plugins)
            {
                _ = plugin.Value.HTTPStartPlugin(HTTPServerConfiguration.APIStaticFolder, (ushort)(HTTPServerConfiguration.DefaultPluginsPort + i));
                i++;
            }
        }

        if (HTTPServerConfiguration.Ports != null && HTTPServerConfiguration.Ports.Count > 0)
        {
            HTTPBag ??= new();

            _ = Processor.TryGetServerIP(HTTPServerConfiguration.Ports.First());

            new List<HTTPServer.Models.Route> { } /*TODO: Make it so we can input custom routes*/.ForEach(route => Processor.AddRoute(route));

            Parallel.ForEach(HTTPServerConfiguration.Ports, port => {
                if (HTTPBag.ContainsKey(port))
                {
                    new Thread(() => {
                        HTTPBag[port].Run(); //Server runs in a dedicated thread seperate from mains thread
                    }).Start();
                }
                else if (TCPUtils.IsTCPPortAvailable(port))
                {
                    HTTPServer.HTTPServer BLL = new(Processor.HandleClient, "0.0.0.0", port, Environment.ProcessorCount * 4);

                    new Thread(() => {
                        BLL.Run(); //Server runs in a dedicated thread seperate from mains thread
                    }).Start();

                    HTTPBag.TryAdd(port, BLL);
                }
            });
        }
        else
        {
            HTTPBag = null;
            LoggerAccessor.LogError("[HTTP] - No ports were found in the server configuration, ignoring server startup...");
        }
    }

    static void Main()
    {
        if (!NetworkLibrary.Extension.Windows.Win32API.IsWindows)
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        else
            TechnitiumLibrary.Net.Firewall.FirewallHelper.CheckFirewallEntries(Assembly.GetEntryAssembly()?.Location);

        LoggerAccessor.SetupLogger("HTTPServer", Directory.GetCurrentDirectory());

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

        IPUtils.GetIPInfos(IPUtils.GetLocalIPAddress().ToString(), IPUtils.GetLocalSubnet());
#endif

        GeoIP.Initialize();

        HTTPServerConfiguration.RefreshVariables(configPath);

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

                            HTTPServerConfiguration.RefreshVariables(configPath);

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