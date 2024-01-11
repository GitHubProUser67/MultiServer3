using Figgle;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;

namespace CustomLogger
{
    public class LoggerAccessor
    {
        public static bool initiated = false;

        public static void SetupLogger(string project)
        {
            try
            {
                Console.Title = project;
                Console.CursorVisible = false;

                Thread.Sleep(100);

                Console.Clear();
            }
            catch (Exception) // If a background or windows service, will assert.
            {

            }

            Console.WriteLine(FiggleFonts.Ogre.Render(project));

            using ILoggerFactory loggerFactory =
                LoggerFactory.Create(builder =>
                {
                    builder.AddSimpleConsole(options => { options.SingleLine = true; });

                    builder.AddProvider(_fileLogger = new FileLoggerProvider(Directory.GetCurrentDirectory() + $"/{project}.log", new FileLoggerOptions()
                    {
                        Append = false,
                        FileSizeLimitBytes = 4294967295, // 4GB (FAT32 max size) - 1 byte
                        MaxRollingFiles = 100
                    }));

                    _fileLogger.MinLevel = LogLevel.Information;
                });

            Logger = loggerFactory.CreateLogger(string.Empty);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT 
                || Environment.OSVersion.Platform == PlatformID.Win32S 
                || Environment.OSVersion.Platform == PlatformID.Win32Windows)
                Task.Run(RessourcesLogger.StartPerfWatcher);

            initiated = true;
        }

        public static void drawTextProgressBar(string? text, int progress, int total)
        {
            if (initiated)
            {
                if (text == null)
                    text = string.Empty;

                try
                {
                    LogInfo($"\n{text}\n");
                    //draw empty progress bar
                    Console.CursorLeft = 0;
                    Console.Write("["); //start
                    Console.CursorLeft = 32;
                    Console.Write("]"); //end
                    Console.CursorLeft = 1;
                    float onechunk = 30.0f / total;

                    int position = 1;
                    if (total == progress)
                    {
                        //draw filled part
                        for (int i = 0; i < 31 && position <= 31; i++)
                        {
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.CursorLeft = position++;
                            Console.Write(" ");
                        }
                    }
                    else
                    {
                        //draw filled part
                        for (int i = 0; i < onechunk * progress; i++)
                        {
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.CursorLeft = position++;
                            Console.Write(" ");
                        }

                        //draw unfilled part
                        for (int i = position; i <= 31; i++)
                        {
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.CursorLeft = position++;
                            Console.Write(" ");
                        }
                    }

                    //draw totals
                    Console.CursorLeft = 35;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write(progress.ToString() + " of " + total.ToString() + "    \n"); //blanks and a newline at the end remove any excess
                }
                catch (Exception)
                {
                    // Not Important.
                }
            }
        }

#pragma warning disable
        public static ILogger Logger { get; set; }

        public static FileLoggerProvider _fileLogger = null;
        public static void LogInfo(string? message) { if (RemoteLogger.IsStarted) RemoteLogger.IncrementAndRotate("Info: " + message); Logger.LogInformation(message, null); }
        public static void LogInfo(string? message, params object[]? args) { if (RemoteLogger.IsStarted) RemoteLogger.IncrementAndRotate("Info: " + message); Logger.LogInformation(message, args); }
        public static void LogInfo(int? message, params object[]? args) { if (RemoteLogger.IsStarted) RemoteLogger.IncrementAndRotate("Info: " + message); Logger.LogInformation(message.ToString(), args); }
        public static void LogInfo(float? message, params object[]? args) { if (RemoteLogger.IsStarted) RemoteLogger.IncrementAndRotate("Info: " + message); Logger.LogInformation(message.ToString(), args); }
        public static void LogWarn(string? message) { if (RemoteLogger.IsStarted) RemoteLogger.IncrementAndRotate("Warn: " + message); Logger.LogWarning(message, null); }
        public static void LogWarn(string? message, params object[]? args) { if (RemoteLogger.IsStarted) RemoteLogger.IncrementAndRotate("Warn: " + message); Logger.LogWarning(message, args); }
        public static void LogWarn(int? message, params object[]? args) { if (RemoteLogger.IsStarted) RemoteLogger.IncrementAndRotate("Warn: " + message); Logger.LogWarning(message.ToString(), args); }
        public static void LogWarn(float? message, params object[]? args) { if (RemoteLogger.IsStarted) RemoteLogger.IncrementAndRotate("Warn: " + message); Logger.LogWarning(message.ToString(), args); }
        public static void LogError(string? message) { if (RemoteLogger.IsStarted) RemoteLogger.IncrementAndRotate("Error: " + message); Logger.LogError(message); }
        public static void LogError(string? message, params object[]? args) { if (RemoteLogger.IsStarted) RemoteLogger.IncrementAndRotate("Error: " + message); Logger.LogError(message, args); }
        public static void LogError(int? message, params object[]? args) { if (RemoteLogger.IsStarted) RemoteLogger.IncrementAndRotate("Error: " + message); Logger.LogError(message.ToString(), args); }
        public static void LogError(float? message, params object[]? args) { if (RemoteLogger.IsStarted) RemoteLogger.IncrementAndRotate("Error: " + message); Logger.LogError(message.ToString(), args); }
        public static void LogError(Exception exception) { if (RemoteLogger.IsStarted) RemoteLogger.IncrementAndRotate("Error: " + exception.ToString()); Logger.LogError(exception.ToString()); }
        public static void LogDebug(string? message) { if (RemoteLogger.IsStarted) RemoteLogger.IncrementAndRotate("Debug: " + message); Logger.LogDebug(message, null); }
        public static void LogDebug(string? message, params object[]? args) { if (RemoteLogger.IsStarted) RemoteLogger.IncrementAndRotate("Debug: " + message); Logger.LogDebug(message, args); }
        public static void LogDebug(int? message, params object[]? args) { if (RemoteLogger.IsStarted) RemoteLogger.IncrementAndRotate("Debug: " + message); Logger.LogDebug(message.ToString(), args); }
        public static void LogDebug(float? message, params object[]? args) { if (RemoteLogger.IsStarted) RemoteLogger.IncrementAndRotate("Debug: " + message); Logger.LogDebug(message.ToString(), args); }
#pragma warning restore
    }
}
