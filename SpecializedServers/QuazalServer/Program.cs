using CustomLogger;
using Newtonsoft.Json.Linq;
using QuazalServer.ServerProcessors;
using System.Runtime;
using System.Security.Cryptography;

public static class QuazalServerConfiguration
{
    public static string ServerBindAddress { get; set; } = CyberBackendLibrary.TCP_IP.IPUtils.GetLocalIPAddress().ToString();
    public static string ServerPublicBindAddress { get; set; } = CyberBackendLibrary.TCP_IP.IPUtils.GetPublicIPAddress();
    public static string EdNetBindAddressOverride { get; set; } = string.Empty;
    public static string QuazalStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/Quazal";
    public static bool UsePublicIP { get; set; } = false;
    public static List<Tuple<int, string>>? BackendServersList { get; set; } = new List<Tuple<int, string>>
                    {
                        Tuple.Create(30201, "yh64s"), // TDUPS2
                        Tuple.Create(30216, "hg7j1"), // TDUPS2BETA
                        Tuple.Create(60106, "w6kAtr3T"), // DFSPC
                        Tuple.Create(61111, "QusaPha9"), // DFSPS3
                        Tuple.Create(62111, "QusaPha9"), // DFSPS3NTSCLOBBY
                        Tuple.Create(60116, "OLjNg84Gh"), // HAWX2PS3
                        Tuple.Create(61121, "q1UFc45UwoyI"), // GRFSPS3
                        Tuple.Create(61126, "cYoqGd4f"), // AC3PS3
                        Tuple.Create(62126, "cYoqGd4f"), // PRIVAC3PS3
                        Tuple.Create(61128, "cYoqGd4f"), // AC3MULTPS3
                        Tuple.Create(62128, "cYoqGd4f"), // AC3PRIVMULTPS3
                        Tuple.Create(61130, "h0rszqTw"), // AC2PS3
                        Tuple.Create(61132, "lON6yKGp"), // SCBLACKLISTPS3
                        Tuple.Create(61134, "ex5LYTJ0"), // WATCHDOGSPS3
                        Tuple.Create(61138, "4TeVtJ7V"), // BGEHDPS3
                        Tuple.Create(61140, "HJb8Ix1M"), // RAYMANLEGENDSPS3
                        Tuple.Create(60001, "ridfebb9"), // RB3
                        Tuple.Create(21032, "8dtRv2oj"), // GRO
                        Tuple.Create(30161, "uG9Kv3p"), // TUROKPS3
                        Tuple.Create(30561, "os4R9pEiy"), // GHOSTBUSTERSPS3
                        Tuple.Create(61136, "pJ3Lsyc2"), // WATCHDOGSWIIU
                    };
    public static List<Tuple<int, int, string>>? RendezVousServersList { get; set; } = new List<Tuple<int, int, string>>
                    {
                        Tuple.Create(30200, 30201, "yh64s"), // TDUPS2
                        Tuple.Create(30215, 30216, "hg7j1"), // TDUPS2BETA
                        Tuple.Create(60105, 60106, "w6kAtr3T"), // DFSPC
                        Tuple.Create(61110, 61111, "QusaPha9"), // DFSPS3
                        Tuple.Create(62110, 62111, "QusaPha9"), // DFSPS3NTSCLOBBY
                        Tuple.Create(60115, 60116, "OLjNg84Gh"), // HAWX2PS3
                        Tuple.Create(61120, 61121, "q1UFc45UwoyI"), // GRFSPS3
                        Tuple.Create(61125, 61126, "cYoqGd4f"), // AC3PS3
                        Tuple.Create(62125, 62126, "cYoqGd4f"), // PRIVAC3PS3
                        Tuple.Create(61127, 61128, "cYoqGd4f"), // AC3MULTPS3
                        Tuple.Create(62127, 62128, "cYoqGd4f"), // AC3PRIVMULTPS3
                        Tuple.Create(61129, 61130, "h0rszqTw"), // AC2PS3
                        Tuple.Create(61131, 61132, "lON6yKGp"), // SCBLACKLISTPS3
                        Tuple.Create(61133, 61134, "ex5LYTJ0"), // WATCHDOGSPS3
                        Tuple.Create(61137, 61138, "4TeVtJ7V"), // BGEHDPS3
                        Tuple.Create(61139, 61140, "HJb8Ix1M"), // RAYMANLEGENDSPS3
                        Tuple.Create(30160, 30161, "uG9Kv3p"), // TUROKPS3
                        Tuple.Create(30560, 30561, "os4R9pEiy"), // GHOSTBUSTERSPS3
                        Tuple.Create(61135, 61136, "pJ3Lsyc2"), // WATCHDOGSWIIU
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
                new JProperty("server_bind_address", ServerBindAddress),
                new JProperty("server_public_bind_address", ServerPublicBindAddress),
                new JProperty("ednet_bind_address_override", EdNetBindAddressOverride),
                new JProperty("quazal_static_folder", QuazalStaticFolder),
                new JProperty("server_public_ip", UsePublicIP),
                new JProperty("backend_servers_list", new JArray(
                    from item in BackendServersList
                    select new JObject(
                        new JProperty("item1", item.Item1),
                        new JProperty("item2", item.Item2)
                    )
                )),
                new JProperty("rendezvous_servers_list", new JArray(
                    from item in RendezVousServersList
                    select new JObject(
                        new JProperty("item1", item.Item1),
                        new JProperty("item2", item.Item2),
                        new JProperty("item3", item.Item3)
                    )
                ))
            ).ToString().Replace("/", "\\\\"));

            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

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
                    BackendServersList = BackendServersListArray.ToObject<List<Tuple<int, string>>>();
            }
            catch
            {

            }
            // Deserialize RendezVousServersList if it exists
            try
            {
                JArray RendezVousServersListArray = config.rendezvous_servers_list;
                if (RendezVousServersListArray != null)
                    RendezVousServersList = RendezVousServersListArray.ToObject<List<Tuple<int, int, string>>>();
            }
            catch
            {

            }
        }
        catch (Exception ex)
        {
            LoggerAccessor.LogWarn($"quazal.json file is malformed (exception: {ex}), using server's default.");
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
    static string configPath = configDir + "quazal.json";
    static bool IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows;
    static Timer? DatabaseUpdate = null;
    static BackendServicesServer? BackendServer;
    static RDVServer? RendezVousServer;

    static void StartOrUpdateServer()
    {
        BackendServer?.Stop();
        RendezVousServer?.Stop();

        if (DatabaseUpdate != null)
        {
            DatabaseUpdate.Dispose();
            DatabaseUpdate = null;
        }

        QuazalServer.RDVServices.UbisoftDatabase.AccountDatabase.InitiateDatabase();

        DatabaseUpdate = new Timer(QuazalServer.RDVServices.UbisoftDatabase.AccountDatabase.ScheduledDatabaseUpdate, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

        BackendServer = new BackendServicesServer();
        RendezVousServer = new RDVServer();
        BackendServer.Start(QuazalServerConfiguration.BackendServersList
                    , 2, new CancellationTokenSource().Token);
        RendezVousServer.Start(QuazalServerConfiguration.RendezVousServersList
                    , 2, new CancellationTokenSource().Token);
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

        LoggerAccessor.SetupLogger("QuazalServer");

        QuazalServer.RDVServices.ServiceFactoryRDV.RegisterRDVServices();

        QuazalServerConfiguration.RefreshVariables(configPath);

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

                                QuazalServerConfiguration.RefreshVariables(configPath);

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