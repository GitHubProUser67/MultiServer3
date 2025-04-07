using CustomLogger;
using NetworkLibrary.Extension;
using NetworkLibrary.TCP_IP;
using Newtonsoft.Json.Linq;
using QuazalServer.ServerProcessors;
using System.Reflection;
using System.Runtime;

public static class QuazalServerConfiguration
{
    public static string ServerBindAddress { get; set; } = IPUtils.GetLocalIPAddress().ToString();
    public static string ServerPublicBindAddress { get; set; } = IPUtils.GetPublicIPAddress();
    public static string EdNetBindAddressOverride { get; set; } = string.Empty;
    public static string QuazalStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/Quazal";
    public static bool UsePublicIP { get; set; } = false;
    public static List<Tuple<int, string, string>>? BackendServersList { get; set; } = new List<Tuple<int, string, string>>
                    {
                        Tuple.Create(30201, "yh64s", "v2Services"), // TDUPS2
                        Tuple.Create(30216, "hg7j1", "v2Services"), // TDUPS2BETA
                        Tuple.Create(30551, "1WguH+y", "v2Services"), // RIDINGCLUBPC
                        //Tuple.Create(60106, "w6kAtr3T", "PCUbisoftServices"), // DFSPC
                        Tuple.Create(61111, "QusaPha9", "PS3UbisoftServices"), // DFSPS3
                        Tuple.Create(62111, "QusaPha9", "PS3UbisoftServices"), // DFSPS3NTSCLOBBY
                        Tuple.Create(60116, "OLjNg84Gh", "PS3UbisoftServices"), // HAWX2PS3
                        Tuple.Create(61121, "q1UFc45UwoyI", "PS3UbisoftServices"), // GRFSPS3
                        Tuple.Create(61126, "cYoqGd4f", "PS3UbisoftServices"), // AC3PS3
                        Tuple.Create(62126, "cYoqGd4f", "PS3UbisoftServices"), // PRIVAC3PS3
                        Tuple.Create(61128, "cYoqGd4f", "PS3UbisoftServices"), // AC3MULTPS3
                        Tuple.Create(62128, "cYoqGd4f", "PS3UbisoftServices"), // AC3PRIVMULTPS3
                        Tuple.Create(61130, "h0rszqTw", "PS3UbisoftServices"), // AC2PS3
                        Tuple.Create(61132, "lON6yKGp", "PS3UbisoftServices"), // SCBLACKLISTPS3
                        Tuple.Create(61134, "ex5LYTJ0", "PS3UbisoftServices"), // WATCHDOGSPS3
                        Tuple.Create(61138, "4TeVtJ7V", "PS3UbisoftServices"), // BGEHDPS3
                        Tuple.Create(61140, "HJb8Ix1M", "PS3UbisoftServices"), // RAYMANLEGENDSPS3
                        //Tuple.Create(60001, "ridfebb9", "RockBand3Services"), // RB3
                        //Tuple.Create(21032, "8dtRv2oj", "PCUbisoftServices"), // GRO
                        Tuple.Create(30161, "uG9Kv3p", "PS3TurokServices"), // TUROKPS3
                        Tuple.Create(30561, "os4R9pEiy", "PS3GhostbustersServices"), // GHOSTBUSTERSPS3
                        //Tuple.Create(61136, "pJ3Lsyc2", "WIIUUbisoftServices"), // WATCHDOGSWIIU
                    };
    public static List<Tuple<int, int, string, string>>? RendezVousServersList { get; set; } = new List<Tuple<int, int, string, string>>
                    {
                        Tuple.Create(30200, 30201, "yh64s", "v2Services"), // TDUPS2
                        Tuple.Create(30215, 30216, "hg7j1", "v2Services"), // TDUPS2BETA
                        Tuple.Create(30550, 30551, "1WguH+y", "v2Services"), // RIDINGCLUBPC
                        //Tuple.Create(60105, 60106, "w6kAtr3T", "PCUbisoftServices"), // DFSPC
                        Tuple.Create(61110, 61111, "QusaPha9", "PS3UbisoftServices"), // DFSPS3
                        Tuple.Create(62110, 62111, "QusaPha9", "PS3UbisoftServices"), // DFSPS3NTSCLOBBY
                        Tuple.Create(60115, 60116, "OLjNg84Gh", "PS3UbisoftServices"), // HAWX2PS3
                        Tuple.Create(61120, 61121, "q1UFc45UwoyI", "PS3UbisoftServices"), // GRFSPS3
                        Tuple.Create(61125, 61126, "cYoqGd4f", "PS3UbisoftServices"), // AC3PS3
                        Tuple.Create(62125, 62126, "cYoqGd4f", "PS3UbisoftServices"), // PRIVAC3PS3
                        Tuple.Create(61127, 61128, "cYoqGd4f", "PS3UbisoftServices"), // AC3MULTPS3
                        Tuple.Create(62127, 62128, "cYoqGd4f", "PS3UbisoftServices"), // AC3PRIVMULTPS3
                        Tuple.Create(61129, 61130, "h0rszqTw", "PS3UbisoftServices"), // AC2PS3
                        Tuple.Create(61131, 61132, "lON6yKGp", "PS3UbisoftServices"), // SCBLACKLISTPS3
                        Tuple.Create(61133, 61134, "ex5LYTJ0", "PS3UbisoftServices"), // WATCHDOGSPS3
                        Tuple.Create(61137, 61138, "4TeVtJ7V", "PS3UbisoftServices"), // BGEHDPS3
                        Tuple.Create(61139, 61140, "HJb8Ix1M", "PS3UbisoftServices"), // RAYMANLEGENDSPS3
                        Tuple.Create(30160, 30161, "uG9Kv3p", "PS3TurokServices"), // TUROKPS3
                        Tuple.Create(30560, 30561, "os4R9pEiy", "PS3GhostbustersServices"), // GHOSTBUSTERSPS3
                        //Tuple.Create(61135, 61136, "pJ3Lsyc2", "WIIUUbisoftServices"), // WATCHDOGSWIIU
                    };

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
            LoggerAccessor.LogWarn("Could not find the quazal.json file, writing and using server's default.");

            Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? Directory.GetCurrentDirectory() + "/static");

            // Write the JObject to a file
            File.WriteAllText(configPath, new JObject(
                new JProperty("config_version", (ushort)2),
                new JProperty("server_bind_address", ServerBindAddress),
                new JProperty("server_public_bind_address", ServerPublicBindAddress),
                new JProperty("ednet_bind_address_override", EdNetBindAddressOverride),
                new JProperty("quazal_static_folder", QuazalStaticFolder),
                new JProperty("server_public_ip", UsePublicIP),
                new JProperty("backend_servers_list", new JArray(
                    from item in BackendServersList
                    select new JObject(
                        new JProperty("item1", item.Item1),
                        new JProperty("item2", item.Item2),
                        new JProperty("item3", item.Item3)
                    )
                )),
                new JProperty("rendezvous_servers_list", new JArray(
                    from item in RendezVousServersList
                    select new JObject(
                        new JProperty("item1", item.Item1),
                        new JProperty("item2", item.Item2),
                        new JProperty("item3", item.Item3),
                        new JProperty("item4", item.Item4)
                    )
                ))
            ).ToString());

            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

            ushort config_version = GetValueOrDefault(config, "config_version", (ushort)0);
            if (config_version == 2)
            {
                ServerBindAddress = GetValueOrDefault(config, "server_bind_address", ServerBindAddress);
                ServerPublicBindAddress = GetValueOrDefault(config, "server_public_bind_address", ServerPublicBindAddress);
                EdNetBindAddressOverride = GetValueOrDefault(config, "ednet_bind_address_override", EdNetBindAddressOverride);
                QuazalStaticFolder = GetValueOrDefault(config, "quazal_static_folder", QuazalStaticFolder);
                UsePublicIP = GetValueOrDefault(config, "server_public_ip", UsePublicIP);
                // Deserialize BackendServersList if it exists
                try
                {
                    JArray BackendServersListArray = config.backend_servers_list;
                    if (BackendServersListArray != null)
                        BackendServersList = BackendServersListArray.ToObject<List<Tuple<int, string, string>>>();
                }
                catch
                {

                }
                // Deserialize RendezVousServersList if it exists
                try
                {
                    JArray RendezVousServersListArray = config.rendezvous_servers_list;
                    if (RendezVousServersListArray != null)
                        RendezVousServersList = RendezVousServersListArray.ToObject<List<Tuple<int, int, string, string>>>();
                }
                catch
                {

                }
            }
            else
                LoggerAccessor.LogWarn($"quazal.json file is outdated, using server's default.");
        }
        catch (Exception ex)
        {
            LoggerAccessor.LogWarn($"quazal.json file is malformed (exception: {ex}), using server's default.");
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
}

class Program
{
    private static string configDir = Directory.GetCurrentDirectory() + "/static/";
    private static string configPath = configDir + "quazal.json";
    private static Timer? DatabaseUpdate = null;
    private static BackendServicesServer? BackendServer;
    private static RDVServer? RendezVousServer;

    private static void StartOrUpdateServer()
    {
        BackendServer?.Stop();
        RendezVousServer?.Stop();
        DatabaseUpdate?.Dispose();
        QuazalServer.RDVServices.ServiceFactoryRDV.ClearServices();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        QuazalServer.RDVServices.UbisoftDatabase.AccountDatabase.InitiateDatabase();

        DatabaseUpdate = new Timer(QuazalServer.RDVServices.UbisoftDatabase.AccountDatabase.ScheduledDatabaseUpdate, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

        BackendServer = new BackendServicesServer();
        RendezVousServer = new RDVServer();
        BackendServer.Start(QuazalServerConfiguration.BackendServersList
                    , 2, new CancellationTokenSource().Token);
        RendezVousServer.Start(QuazalServerConfiguration.RendezVousServersList
                    , 2, new CancellationTokenSource().Token);
    }

    static void Main()
    {
        if (!OtherExtensions.IsWindows)
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        else
            TechnitiumLibrary.Net.Firewall.FirewallHelper.CheckFirewallEntries(Assembly.GetEntryAssembly()?.Location);

        LoggerAccessor.SetupLogger("QuazalServer", Directory.GetCurrentDirectory());

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

        QuazalServerConfiguration.RefreshVariables(configPath);

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

                            QuazalServerConfiguration.RefreshVariables(configPath);

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