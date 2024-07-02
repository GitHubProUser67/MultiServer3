using Figgle;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using Spectre.Console;
using System;
using System.IO;
using System.Threading.Tasks;
using Vertical.SpectreLogger;
using Vertical.SpectreLogger.Destructuring;
using Vertical.SpectreLogger.Formatting;
using Vertical.SpectreLogger.Options;
using Vertical.SpectreLogger.Rendering;

namespace CustomLogger
{
    public class LoggerAccessor
    {
        public static bool initiated = false;

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

            AnsiConsole.Clear();

            AnsiConsole.WriteLine(FiggleFonts.Ogre.Render(project));

            ILoggerFactory factory = LoggerFactory.Create(builder =>
            {
                builder.AddSpectreConsole(options => {

                    options.WriteInBackground();

                    options.ConfigureProfile(LogLevel.Trace, profile =>
                    {
                        profile.OutputTemplate = "[grey35][[{DateTime:MM-dd-yyyy HH:mm:ss}]] trce: [[{ProcessId}[grey39]|[/]{ThreadId}]] {Message}{NewLine}{Exception}[/]";
                        profile
                            .AddTypeStyle<ExceptionRenderer.MethodNameValue>(Color.Grey35)
                            .AddTypeStyle<ExceptionRenderer.ParameterTypeValue>(Color.Grey35)
                            .AddTypeStyle<ExceptionRenderer.SourceLocationValue>(Color.DarkViolet)
                            .AddTypeStyle<ExceptionRenderer.SourceFileValue>(Color.DarkGoldenrod)
                            .AddTypeStyle<ExceptionRenderer.TextValue>(Color.Grey35)
                            .AddTypeStyle<CategoryNameRenderer.Value>(Color.Grey85)
                            .AddTypeStyle<DateTimeRenderer.Value>(Color.Grey85)
                            .DefaultLogValueStyle = $"[{Color.Grey35}]";
                    });

                    options.ConfigureProfile(LogLevel.Debug, profile =>
                    {
                        profile.OutputTemplate = "[grey46][[{DateTime:MM-dd-yyyy HH:mm:ss}]] dbug: [[{ProcessId}[grey39]|[/]{ThreadId}]] {Message}{NewLine}{Exception}[/]";
                        profile
                            .AddTypeStyle(Types.Numerics, Color.DarkViolet)
                            .AddTypeStyle(Types.Characters, Color.DarkOrange3)
                            .AddTypeStyle(Types.Temporal, Color.SlateBlue3)
                            .AddTypeStyle<ExceptionRenderer.ExceptionNameValue>(Color.Grey58)
                            .AddTypeStyle<ExceptionRenderer.ExceptionMessageValue>(Color.DarkOrange3)
                            .AddTypeStyle<ExceptionRenderer.MethodNameValue>(Color.Grey35)
                            .AddTypeStyle<ExceptionRenderer.ParameterTypeValue>(Color.Grey35)
                            .AddTypeStyle<ExceptionRenderer.ParameterNameValue>(Color.MediumPurple4)
                            .AddTypeStyle<ExceptionRenderer.SourceLocationValue>(Color.DarkViolet)
                            .AddTypeStyle<ExceptionRenderer.SourceDirectoryValue>(Color.Grey66)
                            .AddTypeStyle<ExceptionRenderer.SourceFileValue>(Color.DarkGoldenrod)
                            .AddTypeStyle<ExceptionRenderer.TextValue>(Color.Grey35)
                            .AddTypeStyle<DestructuredKeyValue>(Color.Grey70)
                            .AddTypeStyle<CategoryNameRenderer.Value>(Color.Grey85)
                            .AddTypeStyle<DateTimeRenderer.Value>(Color.Grey85)
                            .AddValueStyle(true, Color.DarkSeaGreen4)
                            .AddValueStyle(false, Color.DarkRed_1)
                            .DefaultLogValueStyle = $"[{Color.Grey35}]";
                    });

                    options.ConfigureProfile(LogLevel.Information, profile =>
                    {
                        profile.OutputTemplate = "[grey85][[{DateTime:MM-dd-yyyy HH:mm:ss}]] [green3_1]info[/]: [[{ProcessId}[grey39]|[/]{ThreadId}]] {Message}{NewLine}{Exception}[/]";
                        profile
                            .AddTypeStyle(Types.Numerics, Color.Magenta3_2)
                            .AddTypeStyle(Types.Characters, Color.Gold3_1)
                            .AddTypeStyle(Types.Temporal, Color.SteelBlue3)
                            .AddTypeStyle<ExceptionRenderer.ExceptionNameValue>(Color.Grey85)
                            .AddTypeStyle<ExceptionRenderer.ExceptionMessageValue>(Color.DarkOrange3)
                            .AddTypeStyle<ExceptionRenderer.MethodNameValue>(Color.Grey42)
                            .AddTypeStyle<ExceptionRenderer.ParameterTypeValue>(Color.Grey42)
                            .AddTypeStyle<ExceptionRenderer.ParameterNameValue>(Color.SlateBlue3)
                            .AddTypeStyle<ExceptionRenderer.SourceLocationValue>(Color.Magenta3_2)
                            .AddTypeStyle<ExceptionRenderer.SourceDirectoryValue>(Color.Grey66)
                            .AddTypeStyle<ExceptionRenderer.SourceFileValue>(Color.Gold3_1)
                            .AddTypeStyle<ExceptionRenderer.TextValue>(Color.Grey42)
                            .AddTypeStyle<DestructuredKeyValue>(Color.Grey70)
                            .AddTypeStyle<CategoryNameRenderer.Value>(Color.Grey85)
                            .AddTypeStyle<DateTimeRenderer.Value>(Color.Grey85)
                            .AddValueStyle(true, Color.Green3_1)
                            .AddValueStyle(false, Color.DarkOrange3_1)
                            .DefaultLogValueStyle = $"[{Color.Grey35}]";
                    });

                    options.ConfigureProfile(LogLevel.Warning, profile =>
                    {
                        profile.OutputTemplate = "[grey85][[{DateTime:MM-dd-yyyy HH:mm:ss}]] [gold1]warn[/]: [[{ProcessId}[grey39]|[/]{ThreadId}]] {Message}{NewLine}{Exception}[/]";
                        profile
                            .AddTypeStyle(Types.Numerics, Color.Magenta3_2)
                            .AddTypeStyle(Types.Characters, Color.Gold3_1)
                            .AddTypeStyle(Types.Temporal, Color.SteelBlue3)
                            .AddTypeStyle<ExceptionRenderer.ExceptionNameValue>(Color.Grey85)
                            .AddTypeStyle<ExceptionRenderer.ExceptionMessageValue>(Color.DarkOrange3)
                            .AddTypeStyle<ExceptionRenderer.MethodNameValue>(Color.Grey42)
                            .AddTypeStyle<ExceptionRenderer.ParameterTypeValue>(Color.Grey42)
                            .AddTypeStyle<ExceptionRenderer.ParameterNameValue>(Color.SlateBlue3)
                            .AddTypeStyle<ExceptionRenderer.SourceLocationValue>(Color.Magenta3_2)
                            .AddTypeStyle<ExceptionRenderer.SourceDirectoryValue>(Color.Grey66)
                            .AddTypeStyle<ExceptionRenderer.SourceFileValue>(Color.Gold3_1)
                            .AddTypeStyle<ExceptionRenderer.TextValue>(Color.Grey42)
                            .AddTypeStyle<DestructuredKeyValue>(Color.Grey70)
                            .AddTypeStyle<CategoryNameRenderer.Value>(Color.Grey85)
                            .AddTypeStyle<DateTimeRenderer.Value>(Color.Grey85)
                            .AddValueStyle(true, Color.Green3_1)
                            .AddValueStyle(false, Color.DarkOrange3_1)
                            .DefaultLogValueStyle = $"[{Color.Grey35}]";
                    });

                    options.ConfigureProfile(LogLevel.Error, profile =>
                    {
                        profile.OutputTemplate = "[grey85][[{DateTime:MM-dd-yyyy HH:mm:ss}]] [red1]fail[/]: [[{ProcessId}[grey39]|[/]{ThreadId}]] {Message}{NewLine}{Exception}[/]";
                        profile
                            .AddTypeStyle(Types.Numerics, Color.Magenta3_2)
                            .AddTypeStyle(Types.Characters, Color.Gold3_1)
                            .AddTypeStyle(Types.Temporal, Color.SteelBlue3)
                            .AddTypeStyle<ExceptionRenderer.ExceptionNameValue>(Color.Grey85)
                            .AddTypeStyle<ExceptionRenderer.ExceptionMessageValue>(Color.DarkOrange3)
                            .AddTypeStyle<ExceptionRenderer.MethodNameValue>(Color.Grey42)
                            .AddTypeStyle<ExceptionRenderer.ParameterTypeValue>(Color.Grey42)
                            .AddTypeStyle<ExceptionRenderer.ParameterNameValue>(Color.SlateBlue3)
                            .AddTypeStyle<ExceptionRenderer.SourceLocationValue>(Color.Magenta3_2)
                            .AddTypeStyle<ExceptionRenderer.SourceDirectoryValue>(Color.Grey66)
                            .AddTypeStyle<ExceptionRenderer.SourceFileValue>(Color.Gold3_1)
                            .AddTypeStyle<ExceptionRenderer.TextValue>(Color.Grey42)
                            .AddTypeStyle<DestructuredKeyValue>(Color.Grey70)
                            .AddTypeStyle<CategoryNameRenderer.Value>(Color.Grey85)
                            .AddTypeStyle<DateTimeRenderer.Value>(Color.Grey85)
                            .AddValueStyle(true, Color.Green3_1)
                            .AddValueStyle(false, Color.DarkOrange3_1)
                            .DefaultLogValueStyle = $"[{Color.Grey35}]";
                    });

                    options.ConfigureProfile(LogLevel.Critical, profile =>
                    {
                        profile.OutputTemplate = "[[[red1]{DateTime:MM-dd-yyyy HH:mm:ss}]][/] [white on red1]crit[/]: [[{ProcessId}[grey39]|[/]{ThreadId}]] [red3]{Message}{NewLine}{Exception}[/]";
                        profile
                            .AddTypeStyle(Types.Numerics, Color.Magenta3_2)
                            .AddTypeStyle(Types.Characters, Color.Gold3_1)
                            .AddTypeStyle(Types.Temporal, Color.SteelBlue3)
                            .AddTypeStyle<ExceptionRenderer.ExceptionNameValue>(Color.Grey85)
                            .AddTypeStyle<ExceptionRenderer.ExceptionMessageValue>(Color.DarkOrange3)
                            .AddTypeStyle<ExceptionRenderer.MethodNameValue>(Color.Grey42)
                            .AddTypeStyle<ExceptionRenderer.ParameterTypeValue>(Color.Grey42)
                            .AddTypeStyle<ExceptionRenderer.ParameterNameValue>(Color.SlateBlue3)
                            .AddTypeStyle<ExceptionRenderer.SourceLocationValue>(Color.Magenta3_2)
                            .AddTypeStyle<ExceptionRenderer.SourceDirectoryValue>(Color.Grey66)
                            .AddTypeStyle<ExceptionRenderer.SourceFileValue>(Color.Gold3_1)
                            .AddTypeStyle<ExceptionRenderer.TextValue>(Color.Grey42)
                            .AddTypeStyle<DestructuredKeyValue>(Color.Grey70)
                            .AddTypeStyle<CategoryNameRenderer.Value>(Color.Grey85)
                            .AddTypeStyle<DateTimeRenderer.Value>(Color.Grey85)
                            .AddValueStyle(true, Color.Green3_1)
                            .AddValueStyle(false, Color.DarkOrange3_1)
                            .DefaultLogValueStyle = $"[{Color.Grey35}]";
                    });

                    options.ConfigureProfiles(profile =>
                    {
                        profile.AddTypeFormatters();
                    });

                });
            });

            // Check if the log file is in use by another process, if not create/use one.
            try
            {
                if (File.Exists(logfilePath))
                {
                    using FileStream stream = File.Open(logfilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
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
        public static void LogInfo(string message) { if (initiated) { Logger.LogInformation(message); } }
        public static void LogInfo(string message, params object[] args) {  if (initiated) { Logger.LogInformation(message, args); } }
        public static void LogInfo(int? message, params object[] args) {  if (initiated) { Logger.LogInformation(message.ToString(), args); } }
        public static void LogInfo(float? message, params object[] args) {  if (initiated) { Logger.LogInformation(message.ToString(), args); } }
        public static void LogWarn(string message) { if (initiated) { Logger.LogWarning(message); } }
        public static void LogWarn(string message, params object[] args) { if (initiated) {  Logger.LogWarning(message, args); } }
        public static void LogWarn(int? message, params object[] args) {  if (initiated) { Logger.LogWarning(message.ToString(), args); } }
        public static void LogWarn(float? message, params object[] args) {  if (initiated) { Logger.LogWarning(message.ToString(), args); } }
        public static void LogError(string message) {  if (initiated) { Logger.LogError(message); } }
        public static void LogError(string message, params object[] args) {  if (initiated) { Logger.LogError(message, args); } }
        public static void LogError(int? message, params object[] args) {  if (initiated) { Logger.LogError(message.ToString(), args); } }
        public static void LogError(float? message, params object[] args) {  if (initiated) { Logger.LogError(message.ToString(), args); } }
        public static void LogError(Exception exception) {  if (initiated) { Logger.LogCritical(exception.ToString()); } }
        public static void LogDebug(string message, object? arg = null) {  if (initiated) { Logger.LogDebug(message, arg); } }
        public static void LogDebug(string message, params object[] args) {  if (initiated) { Logger.LogDebug(message, args); } }
        public static void LogDebug(int? message, params object[] args) {  if (initiated) { Logger.LogDebug(message.ToString(), args); } }
        public static void LogDebug(float? message, params object[] args) {  if (initiated) { Logger.LogDebug(message.ToString(), args); } }
#pragma warning restore
    }
}
