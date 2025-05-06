using Figgle;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using System;
using System.Collections.Generic;
using System.IO;

namespace CustomLogger
{
    public static class LoggerAccessor
    {
        public static ILogger Logger { get; set; }

        public static FileLoggerProvider _fileLogger = null;

        private static readonly Dictionary<LogLevel, List<Action<string, object[]>>> _postLogActions
            = new Dictionary<LogLevel, List<Action<string, object[]>>>
            {
                [LogLevel.Information] = new List<Action<string, object[]>>(),
                [LogLevel.Warning] = new List<Action<string, object[]>>(),
                [LogLevel.Error] = new List<Action<string, object[]>>(),
                [LogLevel.Critical] = new List<Action<string, object[]>>(),
                [LogLevel.Debug] = new List<Action<string, object[]>>()
            };

        public static void RegisterPostLogAction(LogLevel level, Action<string, object[]> action)
        {
            if (_postLogActions.ContainsKey(level))
                _postLogActions[level].Add(action);
        }

        private static void RunPostLogActions(LogLevel level, string message, object[] args = null)
        {
            if (_postLogActions.TryGetValue(level, out var actions))
            {
                foreach (var action in actions)
                {
                    try
                    {
                        action?.Invoke(message, args ?? Array.Empty<object>());
                    }
                    catch
                    {
                    }
                }
            }
        }

        public static void SetupLogger(string project, string CurrentDir)
        {
            string logfilePath = Path.Combine(CurrentDir, $"logs/{project}.log");

            try
            {
                Console.Title = project;
                Console.CursorVisible = false;
                Console.Clear();
            }
            catch // If a background or windows service, will assert.
            {

            }

            Console.WriteLine(FiggleFonts.Ogre.Render(project));

            ILoggerFactory factory = LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.TimestampFormat = "[MM-dd-yyyy HH:mm:ss] ";
                });
            });

            try
            {
                if (File.Exists(logfilePath))
                    using (FileStream stream = File.Open(logfilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }

                Directory.CreateDirectory(Path.Combine(CurrentDir, "logs"));

                factory.AddProvider(_fileLogger = new FileLoggerProvider(logfilePath, new FileLoggerOptions()
                {
                    UseUtcTimestamp = true,
                    Append = false,
                    FileSizeLimitBytes = 4294967295, // 4GB (FAT32 max size) - 1 byte
                    MaxRollingFiles = 100
                }));
            }
            catch { }

            Logger = factory.CreateLogger(string.Empty);
        }

#pragma warning disable
        public static void LogInfo(string message)
        {
            Logger.LogInformation(message);
            RunPostLogActions(LogLevel.Information, message);
        }

        public static void LogInfo(object message) => LogInfo(message.ToString());
        public static void LogInfo(string message, params object[] args)
        {
            Logger.LogInformation(message, args);
            RunPostLogActions(LogLevel.Information, message, args);
        }

        public static void LogInfo(object message, params object[] args) => LogInfo(message.ToString(), args);

        public static void LogWarn(string message)
        {
            Logger.LogWarning(message);
            RunPostLogActions(LogLevel.Warning, message);
        }

        public static void LogWarn(object message) => LogWarn(message.ToString());
        public static void LogWarn(string message, params object[] args)
        {
            Logger.LogWarning(message, args);
            RunPostLogActions(LogLevel.Warning, message, args);
        }

        public static void LogWarn(object message, params object[] args) => LogWarn(message.ToString(), args);

        public static void LogError(string message)
        {
            Logger.LogError(message);
            RunPostLogActions(LogLevel.Error, message);
        }

        public static void LogError(object message) => LogError(message.ToString());
        public static void LogError(string message, params object[] args)
        {
            Logger.LogError(message, args);
            RunPostLogActions(LogLevel.Error, message, args);
        }

        public static void LogError(object message, params object[] args) => LogError(message.ToString(), args);

        public static void LogError(Exception exception)
        {
            Logger.LogCritical(exception.ToString());
            RunPostLogActions(LogLevel.Critical, exception.ToString());
        }

        public static void LogDebug(string message)
        {
            Logger.LogDebug(message);
            RunPostLogActions(LogLevel.Debug, message);
        }

        public static void LogDebug(object message) => LogDebug(message.ToString());
        public static void LogDebug(string message, params object[] args)
        {
            Logger.LogDebug(message, args);
            RunPostLogActions(LogLevel.Debug, message, args);
        }

        public static void LogDebug(object message, params object[] args) => LogDebug(message.ToString(), args);

#pragma warning restore
    }
}
