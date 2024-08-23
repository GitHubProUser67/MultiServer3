using CustomLogger;
using System.Diagnostics;

namespace MultiSpy
{
    public class PythonExecEngine
    {
        private static Process? pythonProcess;
        private static StreamReader? standardOutput;

        public PythonExecEngine(string scriptPath, bool isWindows)
        {
            string? pythonPath;

            if (isWindows)
                pythonPath = FindPythonPathWindows();
            else
                pythonPath = FindPythonPathOther();

            if (string.IsNullOrEmpty(pythonPath))
            {
                LoggerAccessor.LogError("[PythonExecEngine] - Python installation not found, quitting the engine...");
                return;
            }

            StartPythonScript(pythonPath, scriptPath);

            _ = Task.Run(ReadPythonOutput);
        }

        private string? FindPythonPathWindows()
        {
            // Check the PATH environment variable for Python installation
            string? envPath = Environment.GetEnvironmentVariable("PATH");

            if (!string.IsNullOrEmpty(envPath))
            {
                foreach (string? path in envPath.Split(';'))
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        string pythonPath = Path.Combine(path, "python.exe");

                        if (File.Exists(pythonPath) && IsPython27Windows(pythonPath))
                            return path; // Return the directory of python.exe
                    }
                }
            }

            return null;
        }

        private string? FindPythonPathOther()
        {
            // Use 'which' command to find python path
            using (Process? process = Process.Start(new ProcessStartInfo
            {
                FileName = "which",
                Arguments = "python2.7",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }))
            {
                if (process != null)
                {
                    string output = process.StandardOutput.ReadToEnd().Trim();

                    process.WaitForExit();

                    if (File.Exists(output))
                        return Path.GetDirectoryName(output);
                }
            }

            return null;
        }

        private bool IsPython27Windows(string pythonExePath)
        {
            try
            {
                using (var process = Process.Start(new ProcessStartInfo
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
                LoggerAccessor.LogError("[PythonExecEngine] - Error while checking if python is of version 2.7: " + ex);
            }

            return false;
        }

        private void StartPythonScript(string pythonPath, string scriptPath)
        {
            LoggerAccessor.LogInfo("[PythonExecEngine] - starting python script: " + scriptPath + " on python directory: " + pythonPath);

            // Create a new process for Python
            pythonProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Path.GetDirectoryName(scriptPath),
                    FileName = Path.Combine(pythonPath, "python.exe"),
                    Arguments = $"\"{scriptPath}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            // Start the process
            pythonProcess.Start();
            pythonProcess.PriorityClass = ProcessPriorityClass.High;

            // Get the standard output
            standardOutput = pythonProcess.StandardOutput;
        }

        private Task ReadPythonOutput()
        {
            // Read the output of the Python script line by line
            string? output;

            try
            {
                while ((output = standardOutput?.ReadLine()) != null)
                {
                    LoggerAccessor.LogInfo("[Python] - " + output);
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError("[PythonExecEngine] - Error while reading python output: " + ex);
            }

            return Task.CompletedTask;
        }

        public void ForceQuitPythonProcess()
        {
            if (pythonProcess != null && !pythonProcess.HasExited)
            {
                try
                {
                    // Kill the Python process
                    pythonProcess.Kill();
                    pythonProcess.WaitForExit();
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError("[PythonExecEngine] - Error terminating the process: " + ex);
                }
            }
        }
    }
}
