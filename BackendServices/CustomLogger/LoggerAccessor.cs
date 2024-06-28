using Figgle;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using Spectre.Console;
using Spectre.Console.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CustomLogger
{
    public class LoggerAccessor
    {
        public static bool initiated = false;

        public static ILogger Logger { get; set; }
        public static ILogger PersistantLogger { get; set; }

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

            AnsiConsole.Clear();

            if (File.Exists(CurrentDir + "/MultiServer.gif"))
            {
                GifProcessor.PrintGifToConsole(CurrentDir + "/MultiServer.gif").Wait();
                AnsiConsole.Clear();
            }

            AnsiConsole.WriteLine(FiggleFonts.Ogre.Render(project));

            Logger = LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole(options => { options.SingleLine = true; options.TimestampFormat = "[MM-dd-yyyy HH:mm:ss] "; });
            }).CreateLogger(string.Empty);

            // Check if the log file is in use by another process, if not create/use one.
            try
            {
                if (File.Exists(logfilePath))
                {
                    using FileStream stream = File.Open(logfilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                }

                Directory.CreateDirectory(CurrentDir + $"/logs");

                PersistantLogger = LoggerFactory.Create(builder =>
                {
                    builder.AddProvider(_fileLogger = new FileLoggerProvider(CurrentDir + $"/logs/{project}.log", new FileLoggerOptions()
                    {
                        UseUtcTimestamp = true,
                        Append = false,
                        FileSizeLimitBytes = 4294967295, // 4GB (FAT32 max size) - 1 byte
                        MaxRollingFiles = 100
                    }));
                    _fileLogger.MinLevel = LogLevel.Information;
                }).CreateLogger(string.Empty);
            }
            catch
            {
               
            }

            initiated = true;

#if DEBUG
            if (Environment.OSVersion.Platform == PlatformID.Win32NT
                || Environment.OSVersion.Platform == PlatformID.Win32S
                || Environment.OSVersion.Platform == PlatformID.Win32Windows)
                _ = Task.Run(ResourceMonitor.StartPerfWatcher);
#endif
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
                    AnsiConsole.Write("["); //start
                    Console.CursorLeft = 32;
                    AnsiConsole.Write("]"); //end
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
                            AnsiConsole.Write(" ");
                        }
                    }
                    else
                    {
                        //draw filled part
                        for (int i = 0; i < onechunk * progress; i++)
                        {
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.CursorLeft = position++;
                            AnsiConsole.Write(" ");
                        }

                        //draw unfilled part
                        for (int i = position; i <= 31; i++)
                        {
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.CursorLeft = position++;
                            AnsiConsole.Write(" ");
                        }
                    }

                    //draw totals
                    Console.CursorLeft = 35;
                    Console.BackgroundColor = ConsoleColor.Black;
                    AnsiConsole.Write(progress.ToString() + " of " + total.ToString() + "    \n"); //blanks and a newline at the end remove any excess
                }
                catch
                {
                    // Not Important.
                }
            }
        }

#pragma warning disable
        public static void LogInfo(string message) { if (initiated) { Logger.LogInformation(message); PersistantLogger?.LogInformation(message); } }
        public static void LogInfo(string message, params object[] args) {  if (initiated) { Logger.LogInformation(message, args); PersistantLogger?.LogInformation(message, args); } }
        public static void LogInfo(int? message, params object[] args) {  if (initiated) { Logger.LogInformation(message.ToString(), args); PersistantLogger?.LogInformation(message.ToString(), args); } }
        public static void LogInfo(float? message, params object[] args) {  if (initiated) { Logger.LogInformation(message.ToString(), args); PersistantLogger?.LogInformation(message.ToString(), args); } }
        public static void LogWarn(string message) { if (initiated) { Logger.LogWarning(message); PersistantLogger?.LogWarning(message); } }
        public static void LogWarn(string message, params object[] args) { if (initiated) {  Logger.LogWarning(message, args); PersistantLogger?.LogWarning(message, args); } }
        public static void LogWarn(int? message, params object[] args) {  if (initiated) { Logger.LogWarning(message.ToString(), args); PersistantLogger?.LogWarning(message.ToString(), args); } }
        public static void LogWarn(float? message, params object[] args) {  if (initiated) { Logger.LogWarning(message.ToString(), args); PersistantLogger?.LogWarning(message.ToString(), args); } }
        public static void LogError(string message) {  if (initiated) { Logger.LogError(message); PersistantLogger?.LogError(message); } }
        public static void LogError(string message, params object[] args) {  if (initiated) { Logger.LogError(message, args); PersistantLogger?.LogError(message, args); } }
        public static void LogError(int? message, params object[] args) {  if (initiated) { Logger.LogError(message.ToString(), args); PersistantLogger?.LogError(message.ToString(), args); } }
        public static void LogError(float? message, params object[] args) {  if (initiated) { Logger.LogError(message.ToString(), args); PersistantLogger?.LogError(message.ToString(), args); } }
        public static void LogError(Exception exception) {  if (initiated) { Logger.LogCritical(exception.ToString()); PersistantLogger?.LogCritical(exception.ToString()); } }
        public static void LogDebug(string message, object? arg = null) {  if (initiated) { Logger.LogDebug(message, arg); PersistantLogger?.LogDebug(message, arg); } }
        public static void LogDebug(string message, params object[] args) {  if (initiated) { Logger.LogDebug(message, args); PersistantLogger?.LogDebug(message, args); } }
        public static void LogDebug(int? message, params object[] args) {  if (initiated) { Logger.LogDebug(message.ToString(), args); PersistantLogger?.LogDebug(message.ToString(), args); } }
        public static void LogDebug(float? message, params object[] args) {  if (initiated) { Logger.LogDebug(message.ToString(), args); PersistantLogger?.LogDebug(message.ToString(), args); } }
        public static void LogJson(string message, string header = "JSON Data")
        {
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

            if (initiated)
                PersistantLogger?.LogInformation($"{header.Replace("[[", "[").Replace("]]", "]")} (" + message + ')');
        }
#pragma warning restore
    }
}
