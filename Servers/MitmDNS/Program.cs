using CustomLogger;
using MitmDNS;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using NetworkLibrary.Extension;
using NetworkLibrary.SNMP;
using NetworkLibrary;
using Microsoft.Extensions.Logging;

public static class MitmDNSServerConfiguration
{
    public static string DNSConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/routes.txt";
    public static string DNSOnlineConfig { get; set; } = string.Empty;
    public static bool DNSAllowUnsafeRequests { get; set; } = true;
    public static bool EnableAdguardFiltering { get; set; } = false;
    public static bool EnableDanPollockHosts { get; set; } = false;
    public static List<string> BannedIPs { get; set; }

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
            LoggerAccessor.LogWarn("Could not find the dns.json file, writing and using server's default.");

            Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? Directory.GetCurrentDirectory() + "/static");

            // Write the JObject to a file
            File.WriteAllText(configPath, new JObject(
                new JProperty("online_routes_config", DNSOnlineConfig),
                new JProperty("routes_config", DNSConfig),
                new JProperty("allow_unsafe_requests", DNSAllowUnsafeRequests),
                new JProperty("enable_adguard_filtering", EnableAdguardFiltering),
                new JProperty("enable_dan_pollock_hosts", EnableDanPollockHosts),
                new JProperty("BannedIPs", new JArray(BannedIPs ?? new List<string> { }))
            ).ToString());

            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

            DNSOnlineConfig = GetValueOrDefault(config, "online_routes_config", DNSOnlineConfig);
            DNSConfig = GetValueOrDefault(config, "routes_config", DNSConfig);
            DNSAllowUnsafeRequests = GetValueOrDefault(config, "allow_unsafe_requests", DNSAllowUnsafeRequests);
            EnableAdguardFiltering = GetValueOrDefault(config, "enable_adguard_filtering", EnableAdguardFiltering);
            EnableDanPollockHosts = GetValueOrDefault(config, "enable_dan_pollock_hosts", EnableDanPollockHosts);
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
            LoggerAccessor.LogWarn($"dns.json file is malformed (exception: {ex}), using server's default.");
        }
    }

    // Helper method to get a value or default value if not present
    private static T GetValueOrDefault<T>(dynamic obj, string propertyName, T defaultValue)
    {
        try
        {
            if (obj != null)
            {
                if (obj is JObject jObject)
                {
                    if (jObject.TryGetValue(propertyName, out JToken value))
                    {
                        T returnvalue = value.ToObject<T>();
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
}

class Program
{
    private static string configDir = Directory.GetCurrentDirectory() + "/static/";
    private static string configPath = configDir + "dns.json";
    private static string configNetworkLibraryPath = configDir + "NetworkLibrary.json";
    private static string DNSconfigMD5 = string.Empty;
    private static Task DNSThread = null;
    private static Task DNSRefreshThread = null;
    private static SnmpTrapSender trapSender = null;
    private static DNSUdpServer Server = null;
    private static readonly FileSystemWatcher dnswatcher = new FileSystemWatcher();

    // Event handler for DNS change event
    private static void OnDNSChanged(object source, FileSystemEventArgs e)
    {
        try
        {
            dnswatcher.EnableRaisingEvents = false;

            LoggerAccessor.LogInfo($"DNS Routes File {e.FullPath} has been changed, Routes Refresh at - {DateTime.Now}");

            // Sleep a little to let file-system time to write the changes to the file.
            Thread.Sleep(6000);

            DNSconfigMD5 = ComputeMD5FromFile(MitmDNSServerConfiguration.DNSConfig);

            while (DNSRefreshThread != null)
            {
                LoggerAccessor.LogWarn("[DNS] - Waiting for previous DNS refresh Task to finish...");
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
        Server?.Stop();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        if (MitmDNSServerConfiguration.EnableAdguardFiltering)
            _ = DNSResolver.adChecker.DownloadAndParseFilterListAsync();
        if (MitmDNSServerConfiguration.EnableDanPollockHosts)
            _ = DNSResolver.danChecker.DownloadAndParseFilterListAsync();

        dnswatcher.Path = Path.GetDirectoryName(MitmDNSServerConfiguration.DNSConfig) ?? configDir;
        dnswatcher.Filter = Path.GetFileName(MitmDNSServerConfiguration.DNSConfig);

        if (File.Exists(MitmDNSServerConfiguration.DNSConfig))
        {
            string MD5 = ComputeMD5FromFile(MitmDNSServerConfiguration.DNSConfig);

            if (!MD5.Equals(DNSconfigMD5))
            {
                DNSconfigMD5 = MD5;

                while (DNSRefreshThread != null)
                {
                    LoggerAccessor.LogWarn("[DNS] - Waiting for previous DNS refresh Task to finish...");
                    Thread.Sleep(6000);
                }

                DNSRefreshThread = RefreshDNS();
                DNSRefreshThread.Dispose();
                DNSRefreshThread = null;
            }
        }

        _ = InternetProtocolUtils.TryGetServerIP(out DNSResolver.ServerIp);

        if (Server == null)
            Server = new(Environment.ProcessorCount * 4);
        else
            Server.Start();
    }

    private static Task RefreshDNS()
    {
        if (DNSThread != null && !DNSConfigProcessor.Initiated)
        {
            while (!DNSConfigProcessor.Initiated)
            {
                LoggerAccessor.LogWarn("[DNS] - Waiting for previous config assignement Task to finish...");
                Thread.Sleep(6000);
            }
        }

        DNSThread = Task.Run(DNSConfigProcessor.InitDNSSubsystem);

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

        LoggerAccessor.SetupLogger("MitmDNS", Directory.GetCurrentDirectory());

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
#endif

        NetworkLibraryConfiguration.RefreshVariables(configNetworkLibraryPath);

        if (NetworkLibraryConfiguration.EnableSNMPReports)
        {
            trapSender = new SnmpTrapSender(NetworkLibraryConfiguration.SNMPHashAlgorithm.Name, NetworkLibraryConfiguration.SNMPTrapHost, NetworkLibraryConfiguration.SNMPUserName,
                    NetworkLibraryConfiguration.SNMPAuthPassword, NetworkLibraryConfiguration.SNMPPrivatePassword,
                    NetworkLibraryConfiguration.SNMPEnterpriseOid);

            if (trapSender.report != null)
            {
                LoggerAccessor.RegisterPostLogAction(LogLevel.Information, (msg, args) =>
                {
                    if (NetworkLibraryConfiguration.EnableSNMPReports)
                        trapSender!.SendInfo(msg);
                });

                LoggerAccessor.RegisterPostLogAction(LogLevel.Warning, (msg, args) =>
                {
                    if (NetworkLibraryConfiguration.EnableSNMPReports)
                        trapSender!.SendWarn(msg);
                });

                LoggerAccessor.RegisterPostLogAction(LogLevel.Error, (msg, args) =>
                {
                    if (NetworkLibraryConfiguration.EnableSNMPReports)
                        trapSender!.SendCrit(msg);
                });

                LoggerAccessor.RegisterPostLogAction(LogLevel.Critical, (msg, args) =>
                {
                    if (NetworkLibraryConfiguration.EnableSNMPReports)
                        trapSender!.SendCrit(msg);
                });
#if DEBUG
                LoggerAccessor.RegisterPostLogAction(LogLevel.Debug, (msg, args) =>
                {
                    if (NetworkLibraryConfiguration.EnableSNMPReports)
                        trapSender!.SendInfo(msg);
                });
#endif
            }
        }

        MitmDNSServerConfiguration.RefreshVariables(configPath);

        StartOrUpdateServer();

        dnswatcher.EnableRaisingEvents = true;

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

                            MitmDNSServerConfiguration.RefreshVariables(configPath);

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