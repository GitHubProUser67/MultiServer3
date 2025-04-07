using Figgle;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using Spectre.Console;
using Spectre.Console.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CustomLogger
{
    public class LoggerAccessor
    {
        private static bool initiated = false;

        public static void SetupLogger(string project)
        {
            try
            {
                Console.Title = project;
                Console.CursorVisible = false;

                Thread.Sleep(100);

                Console.Clear();
            }
            catch // If a background or windows service, will assert.
            {

            }

            Directory.CreateDirectory(Directory.GetCurrentDirectory() + $"/logs");

            Console.WriteLine(FiggleFonts.Ogre.Render(project));

            Logger = LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole(options => { options.SingleLine = true; options.TimestampFormat = "[MM-dd-yyyy HH:mm:ss] "; });
                builder.AddProvider(_fileLogger = new FileLoggerProvider(Directory.GetCurrentDirectory() + $"/logs/{project}.log", new FileLoggerOptions()
                {
                    Append = false,
                    FileSizeLimitBytes = 4294967295, // 4GB (FAT32 max size) - 1 byte
                    MaxRollingFiles = 100
                }));

                _fileLogger.MinLevel = LogLevel.Information;
            }).CreateLogger(string.Empty);
#if DEBUG
            if (Environment.OSVersion.Platform == PlatformID.Win32NT
                || Environment.OSVersion.Platform == PlatformID.Win32S
                || Environment.OSVersion.Platform == PlatformID.Win32Windows)
                _ = Task.Run(ResourceMonitor.StartPerfWatcher);
#endif
            initiated = true;
        }

        public static void DrawTextProgressBar(string text, int progress, int total, bool warn = false)
        {
            if (initiated)
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
        }

#pragma warning disable
        public static ILogger Logger { get; set; }

        public static FileLoggerProvider _fileLogger = null;
        public static void LogInfo(string message) { Logger.LogInformation(message, null); }
        public static void LogInfo(string message, params object[] args) { Logger.LogInformation(message, args); }
        public static void LogInfo(int? message, params object[] args) { Logger.LogInformation(message.ToString(), args); }
        public static void LogInfo(float? message, params object[] args) { Logger.LogInformation(message.ToString(), args); }
        public static void LogWarn(string message) { Logger.LogWarning(message, null); }
        public static void LogWarn(string message, params object[] args) { Logger.LogWarning(message, args); }
        public static void LogWarn(int? message, params object[] args) { Logger.LogWarning(message.ToString(), args); }
        public static void LogWarn(float? message, params object[] args) { Logger.LogWarning(message.ToString(), args); }
        public static void LogError(string message) { Logger.LogError(message); }
        public static void LogError(string message, params object[] args) { Logger.LogError(message, args); }
        public static void LogError(int? message, params object[] args) { Logger.LogError(message.ToString(), args); }
        public static void LogError(float? message, params object[] args) { Logger.LogError(message.ToString(), args); }
        public static void LogError(Exception exception) { Logger.LogCritical(exception.Message.ToString()); }
        public static void LogDebug(string message) { Logger.LogDebug(message, null); }
        public static void LogDebug(string message, params object[] args) { Logger.LogDebug(message, args); }
        public static void LogDebug(int? message, params object[] args) { Logger.LogDebug(message.ToString(), args); }
        public static void LogDebug(float? message, params object[] args) { Logger.LogDebug(message.ToString(), args); }
        public static void LogJson(string message, string header = "JSON Data") {
            AnsiConsole.Write(
                new Panel(new JsonText(message)
                    .BracesColor(Color.Red)
                    .BracketColor(Color.Green)
                    .ColonColor(Color.Blue)
                    .CommaColor(Color.Red)
                    .StringColor(Color.Green)
                    .NumberColor(Color.Blue)
                    .BooleanColor(Color.Red)
                    .NullColor(Color.Green))
                    .Header($"[[{DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss")}]] " + header)
                    .Collapse()
                    .RoundedBorder()
                    .BorderColor(ConsoleColor.Gray));
        }
#pragma warning restore
    }
}
