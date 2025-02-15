using CustomLogger;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Diagnostics;
using System.Text;

namespace MultiSpy.Servers
{
    internal class ChatServer
    {
        private static readonly string? pythonPath = FindPythonPath();

        private string? pythonScript;

        public Thread? Thread;

        private ScriptEngine? engine;

        public ChatServer()
        {
            if (!NetworkLibrary.Extension.Windows.Win32API.IsWindows)
            {
                LoggerAccessor.LogError("[ChatServer] - Server is only supported on Windows...");
                return;
            }
            else if (string.IsNullOrEmpty(pythonPath))
            {
                LoggerAccessor.LogError("[ChatServer] - Python installation not found, quitting the engine...");
                return;
            }

            string serverScript = MultiSpyServerConfiguration.ChatServerPath;
			
            if (!string.IsNullOrEmpty(serverScript) && File.Exists(serverScript))
            {
                if (serverScript.EndsWith(".EdgeZlib"))
                    pythonScript = Encoding.UTF8.GetString(CompressionLibrary.Edge.Zlib.EdgeZlibDecompress(File.ReadAllBytes(serverScript)).Result);
                else
                    pythonScript = File.ReadAllText(serverScript);
            }
            else
            {
                LoggerAccessor.LogError("[ChatServer] - Python script not found, quitting the engine...");
                return;
            }
			
            engine = Python.CreateEngine();
            engine.SetSearchPaths(new List<string>(engine.GetSearchPaths()) { pythonPath + "\\Lib" });

            Thread = new Thread(StartServer)
            {
                Name = "Chat Thread"
            };
            Thread.Start();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    engine?.Runtime.Shutdown();
                    engine = null;
                }
            }
            catch (Exception)
            {
            }
        }

        ~ChatServer()
        {
            Dispose(false);
        }

        private void StartServer(object? parameter)
        {
            LoggerAccessor.LogInfo("[ChatServer] - Starting Chat Server");

            try
            {
                engine.Execute(pythonScript, engine.CreateScope());
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[ChatServer] - Python script thrown an assertion: {ex}");
            }
        }

        private static string? FindPythonPath()
        {
            // Check the PATH environment variable for Python installation
            string? envPath = Environment.GetEnvironmentVariable("PATH");

            if (!string.IsNullOrEmpty(envPath))
            {
                foreach (string? path in envPath.Split(';'))
                {
                    if (!string.IsNullOrEmpty(path) && path.EndsWith("python.exe", StringComparison.InvariantCultureIgnoreCase)
                        && File.Exists(path) && IsPython27Windows(path))
                        return Path.GetDirectoryName(path);
                }
            }

            return null;
        }

        private static bool IsPython27Windows(string pythonExePath)
        {
            try
            {
                using (Process? process = Process.Start(new ProcessStartInfo
                {
                    FileName = pythonExePath,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }))
                {
                    if (process != null)
                    {
                        string output = process.StandardError.ReadToEnd().Trim(); // Version info is usually in stderr

                        process.WaitForExit();

                        // Check if the output indicates Python 2.7
                        return output.StartsWith("Python 2.7");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError("[ChatServer] - Error while checking if python is of version 2.7: " + ex);
            }

            return false;
        }
    }
}
