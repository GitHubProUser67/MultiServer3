using CustomLogger;
using STOMPServer;
using System.Runtime;

class Program
{
    static void Main()
    {
        if (!BackendProject.MiscUtils.IsWindows())
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("STOMPServer");

        _ = Task.Run(() => new STOMPProcessor().StartSTOMP(new CancellationTokenSource().Token));

        if (BackendProject.MiscUtils.IsWindows())
        {
            while (true)
            {
                LoggerAccessor.LogInfo("Press any key to shutdown the server. . .");

                Console.ReadLine();

                LoggerAccessor.LogWarn("Are you sure you want to shut down the server? [y/N]");
                char input = char.ToLower(Console.ReadKey().KeyChar);

                if (input == 'y')
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