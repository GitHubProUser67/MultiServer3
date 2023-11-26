using System.Diagnostics;
using System.Security.Principal;

namespace GraphicalUserInterface
{ 
    // A simple process manager class to keep track of processes made using ChatGPT
    public static class ProcessManager
    {
        private static readonly object lockObject = new();
        private static readonly Dictionary<string, Process> processes = new();

        public static Task StartupProgram(string exeName, string appid)
        {
            string? exePath = FindExecutable(exeName);
            if (!string.IsNullOrEmpty(exePath))
            {
                // Start the process in the background
                ProcessStartInfo startInfo = new()
                {
                    FileName = exePath,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                if (IsWindows())
                {
                    bool IsAdmin = IsAdministrator();
                    if (exeName.ToLower().Contains("svo.exe") && !IsAdmin)
                    {
                        Console.WriteLine("SVO cannot start without admin rights, restart the application as an administrator.");
                        return Task.CompletedTask;
                    }
                    else
                    {
                        if (IsAdmin)
                            startInfo.Verb = "runas";
                    }
                }

                Process process = new()
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true
                };

                process.OutputDataReceived += (sender, e) =>
                {
                    // Handle process output
                    if (!string.IsNullOrEmpty(e.Data))
                        Console.WriteLine($"[{appid}] Output: {e.Data}");
                };

                process.Exited += (sender, e) =>
                {
                    // Handle process exit
                    Console.WriteLine($"[{appid}] Process exited with code {process.ExitCode}");
                };

                // Start the process
                process.Start();
                process.BeginOutputReadLine();

                // Store appid and process information for later shutdown
                RegisterProcess(appid, process);
            }
            else
                Console.WriteLine($"Executable [{exeName}] not found in the current directory or its subdirectories.");

            return Task.CompletedTask;
        }

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool IsWindows()
        {
            var os = Environment.OSVersion;
            return os.Platform == PlatformID.Win32NT;
        }

        private static string? FindExecutable(string exeName)
        {
            string exePath = Directory.GetCurrentDirectory() + $"/{exeName}";

            if (File.Exists(exePath))
                return exePath;

            return null; // Executable not found
        }

        public static void RegisterProcess(string appid, Process process)
        {
            lock (lockObject)
            {
                processes[appid] = process;
            }
        }

        public static Task ShutdownProcess(string appid)
        {
            lock (lockObject)
            {
                if (processes.TryGetValue(appid, out Process process))
                {
                    Console.WriteLine($"Shutting down [{appid}]...");
                    // Add any additional cleanup or shutdown logic here
                    process.CloseMainWindow(); // Try to close the main window gracefully
                    Thread.Sleep(2000); // Wait for the process to exit gracefully

                    if (!process.HasExited)
                    {
                        // If the process didn't exit gracefully, kill it
                        Console.WriteLine($"Forcefully terminating [{appid}]...");
                        process.Kill();
                    }

                    // Remove the process from the manager
                    processes.Remove(appid);
                }
                else
                    Console.WriteLine($"No process found with appid: {appid}");
            }

            return Task.CompletedTask;
        }
    }
}
