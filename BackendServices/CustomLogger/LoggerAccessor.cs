using Figgle;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CustomLogger
{
    public class LoggerAccessor
    {
        public static ILogger Logger { get; set; }

        public static FileLoggerProvider _fileLogger = null;

        public static void SetupLogger(string project, string CurrentDir)
        {
            string logfilePath = CurrentDir + $"/logs/{project}.log";

            try
            {
                Console.Title = project;
                Console.CursorVisible = false;
            }
            catch // If a background or windows service, will assert.
            {

            }

            Console.Clear();

            Console.WriteLine(FiggleFonts.Ogre.Render(project));

            ILoggerFactory factory = LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole(options => { options.SingleLine = true; options.TimestampFormat = "[MM-dd-yyyy HH:mm:ss] "; });
            });

            // Check if the log file is in use by another process, if not create/use one.
            try
            {
                if (File.Exists(logfilePath))
                {
                    using (FileStream stream = File.Open(logfilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        
                    }
                }

                Directory.CreateDirectory(CurrentDir + $"/logs");

                factory.AddProvider(_fileLogger = new FileLoggerProvider(CurrentDir + $"/logs/{project}.log", new FileLoggerOptions()
                {
                    UseUtcTimestamp = true,
                    Append = false,
                    FileSizeLimitBytes = 4294967295, // 4GB (FAT32 max size) - 1 byte
                    MaxRollingFiles = 100
                }));
            }
            catch
            {
                // Not Important.
            }

            Logger = factory.CreateLogger(string.Empty);

#if DEBUG
            if (Environment.OSVersion.Platform == PlatformID.Win32NT
                || Environment.OSVersion.Platform == PlatformID.Win32S
                || Environment.OSVersion.Platform == PlatformID.Win32Windows)
                _ = Task.Run(ResourceMonitor.StartPerfWatcher);
#endif
        }

        public static void DrawTextProgressBar(string text, int progress, int total, bool warn = false)
        {
            if (string.IsNullOrEmpty(text))
                text = string.Empty;

                try
                {
                    if (warn)
                        LogWarn($"\n{text}\n");
                    else
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
                catch
                {
                    // Not Important.
                }
        }

#pragma warning disable
        public static void LogInfo(string message) { Logger.LogInformation(message); }
        public static void LogInfo(string message, params object[] args) {  Logger.LogInformation(message, args); }
        public static void LogInfo(int? message, params object[] args) {  Logger.LogInformation(message.ToString(), args); }
        public static void LogInfo(float? message, params object[] args) {  Logger.LogInformation(message.ToString(), args); }
        public static void LogWarn(string message) { Logger.LogWarning(message); }
        public static void LogWarn(string message, params object[] args) { Logger.LogWarning(message, args); }
        public static void LogWarn(int? message, params object[] args) {  Logger.LogWarning(message.ToString(), args); }
        public static void LogWarn(float? message, params object[] args) {  Logger.LogWarning(message.ToString(), args); }
        public static void LogError(string message) {  Logger.LogError(message); }
        public static void LogError(string message, params object[] args) {  Logger.LogError(message, args); }
        public static void LogError(int? message, params object[] args) {  Logger.LogError(message.ToString(), args); }
        public static void LogError(float? message, params object[] args) {  Logger.LogError(message.ToString(), args); }
        public static void LogError(Exception exception) {  Logger.LogCritical(exception.ToString()); }
        public static void LogDebug(string message, object arg = null) {  Logger.LogDebug(message, arg); }
        public static void LogDebug(string message, params object[] args) {  Logger.LogDebug(message, args); }
        public static void LogDebug(int? message, params object[] args) {  Logger.LogDebug(message.ToString(), args); }
        public static void LogDebug(float? message, params object[] args) {  Logger.LogDebug(message.ToString(), args); }
#pragma warning restore
    }
}
