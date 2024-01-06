using Figgle;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;

namespace CustomLogger
{
    public class LoggerAccessor
    {
        public static void SetupLogger(string project)
        {
            try
            {
                Console.Title = project;
                Console.CursorVisible = false;

                Thread.Sleep(100);

                Console.Clear();
            }
            catch (Exception)
            {

            }

            Console.WriteLine(FiggleFonts.Ogre.Render(project));

            var loggingOptions = new FileLoggerOptions()
            {
                Append = false,
                FileSizeLimitBytes = 4294967295, // 4GB (FAT32 max size) - 1 byte
                MaxRollingFiles = 100
            };

            using ILoggerFactory loggerFactory =
                LoggerFactory.Create(builder =>
                {
                    builder.AddSimpleConsole(options => { options.SingleLine = true; });

                    builder.AddProvider(_fileLogger = new FileLoggerProvider(Directory.GetCurrentDirectory() + $"/{project}.log", loggingOptions));
                    _fileLogger.MinLevel = LogLevel.Information;
                });

            Logger = loggerFactory.CreateLogger(string.Empty);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT 
                || Environment.OSVersion.Platform == PlatformID.Win32S 
                || Environment.OSVersion.Platform == PlatformID.Win32Windows)
                Task.Run(RessourcesLogger.StartPerfWatcher);
        }

#pragma warning disable
        public static ILogger Logger { get; set; }

        public static FileLoggerProvider _fileLogger = null;
        public static void LogInfo(string? message) { Logger.LogInformation(message, null); }
        public static void LogInfo(string? message, params object[]? args) { Logger.LogInformation(message, args); }
        public static void LogInfo(int? message, params object[]? args) { Logger.LogInformation(message.ToString(), args); }
        public static void LogInfo(float? message, params object[]? args) { Logger.LogInformation(message.ToString(), args); }
        public static void LogWarn(string? message) { Logger.LogWarning(message, null); }
        public static void LogWarn(string? message, params object[]? args) { Logger.LogWarning(message, args); }
        public static void LogWarn(int? message, params object[]? args) { Logger.LogWarning(message.ToString(), args); }
        public static void LogWarn(float? message, params object[]? args) { Logger.LogWarning(message.ToString(), args); }
        public static void LogError(string? message) { Logger.LogError(message); }
        public static void LogError(string? message, params object[]? args) { Logger.LogError(message, args); }
        public static void LogError(int? message, params object[]? args) { Logger.LogError(message.ToString(), args); }
        public static void LogError(float? message, params object[]? args) { Logger.LogError(message.ToString(), args); }
        public static void LogError(Exception exception) { Logger.LogError(exception.ToString()); }
        public static void LogDebug(string? message) { Logger.LogDebug(message, null); }
        public static void LogDebug(string? message, params object[]? args) { Logger.LogDebug(message, args); }
        public static void LogDebug(int? message, params object[]? args) { Logger.LogDebug(message.ToString(), args); }
        public static void LogDebug(float? message, params object[]? args) { Logger.LogDebug(message.ToString(), args); }
#pragma warning restore
    }
}
