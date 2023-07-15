using DotNetty.Common.Internal.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using NReco.Logging.File;
using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;
using System.Runtime.Serialization;

namespace PSMultiServer.SRC_Addons.MEDIUS.Server.Common.Logging
{
    public class GetLogs
    {
        public static LogSettings Logging { get; set; } = new LogSettings();

        public static FileLoggerProvider _fileLogger = null;

        public static void StartPooling()
        {
            // Add file logger if path is valid
            if (new FileInfo(LogSettings.Singleton.LogPath)?.Directory?.Exists ?? false)
            {
                var loggingOptions = new FileLoggerOptions()
                {
                    Append = false,
                    FileSizeLimitBytes = LogSettings.Singleton.RollingFileSize,
                    MaxRollingFiles = LogSettings.Singleton.RollingFileCount
                };
                InternalLoggerFactory.DefaultFactory.AddProvider(_fileLogger = new FileLoggerProvider(LogSettings.Singleton.LogPath, loggingOptions));
                _fileLogger.MinLevel = Logging.LogLevel;
            }

            InternalLoggerFactory.DefaultFactory.AddProvider(new ConsoleLoggerProvider((s, level) => level >= LogSettings.Singleton.LogLevel, true));

            return;
        }

    }

    public class LogSettings
    {
        /// <summary>
        /// 
        /// </summary>
        public static LogSettings Singleton { get; set; } = null;

        /// <summary>
        /// Path to the log file.
        /// </summary>
        public string LogPath { get; set; } = Directory.GetCurrentDirectory() + "/medius.log";

        /// <summary>
        /// Whether to output metric information.
        /// </summary>
        public bool LogMetrics { get; set; } = false;

        /// <summary>
        /// Whether to also log to the console.
        /// </summary>
        public bool LogToConsole { get; set; } = false;

        /// <summary>
        /// Size in bytes for each log file.
        /// </summary>
        public int RollingFileSize = 1024 * 1024 * 1;

        /// <summary>
        /// Max number of files before rolling back to the first log file.
        /// </summary>
        public int RollingFileCount = 100;

        /// <summary>
        /// Log level.
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// Collection of RT messages to print out
        /// </summary>
        public string[] RtLogFilter { get; set; } = Enum.GetNames(typeof(RT_MSG_TYPE));

        /// <summary>
        /// Collection of Medius Lobby messages to print out
        /// </summary>
        public string[] MediusLobbyLogFilter { get; set; } = Enum.GetNames(typeof(MediusLobbyMessageIds));

        /// <summary>
        /// Collection of Medius Lobby Ext messages to print out
        /// </summary>
        public string[] MediusLobbyExtLogFilter { get; set; } = Enum.GetNames(typeof(MediusLobbyExtMessageIds));

        /// <summary>
        /// Collection of Medius Lobby Ext messages to print out
        /// </summary>
        public string[] MediusMGCLLogFilter { get; set; } = Enum.GetNames(typeof(MediusMGCLMessageIds));

        /// <summary>
        /// Collection of Medius Lobby Ext messages to print out
        /// </summary>
        public string[] MediusDMEExtLogFilter { get; set; } = Enum.GetNames(typeof(MediusDmeMessageIds));

        /// <summary>
        /// Collection of Medius Plugin Zipper messages to print out
        /// </summary>
        public string[] MediusNetMessageTypesLogFilter { get; set; } = Enum.GetNames(typeof(NetMessageTypeIds));

        /// <summary>
        /// Collection of Medius GHS Opcodes
        /// </summary>
        public string[] MediusGhsOpcodeFilter { get; set; } = Enum.GetNames(typeof(GhsOpcode));

        /// <summary>
        /// Internal preprocessed collection of message id filters.
        /// </summary>
        private Dictionary<RT_MSG_TYPE, bool> _rtLogFilters = new Dictionary<RT_MSG_TYPE, bool>();
        private Dictionary<MediusDmeMessageIds, bool> _dmeLogFilters = new Dictionary<MediusDmeMessageIds, bool>();
        private Dictionary<MediusLobbyMessageIds, bool> _lobbyLogFilters = new Dictionary<MediusLobbyMessageIds, bool>();
        private Dictionary<MediusMGCLMessageIds, bool> _mgclLogFilters = new Dictionary<MediusMGCLMessageIds, bool>();
        private Dictionary<MediusLobbyExtMessageIds, bool> _lobbyExtLogFilters = new Dictionary<MediusLobbyExtMessageIds, bool>();
        private Dictionary<NetMessageTypeIds, bool> _netLogFilters = new Dictionary<NetMessageTypeIds, bool>();
        private Dictionary<GhsOpcode, bool> _ghsOpsLogFilters = new Dictionary<GhsOpcode, bool>();

        /// <summary>
        /// Whether or not the given RT message id should be logged
        /// </summary>
        public bool IsLog(RT_MSG_TYPE msgType)
        {
            return _rtLogFilters.TryGetValue(msgType, out var r) && r;
        }

        /// <summary>
        /// Whether or not the given Medius lobby dme message id should be logged
        /// </summary>
        public bool IsLog(MediusDmeMessageIds msgType)
        {
            return _dmeLogFilters.TryGetValue(msgType, out var r) && r;
        }

        /// <summary>
        /// Whether or not the given Medius lobby message id should be logged
        /// </summary>
        public bool IsLog(MediusLobbyMessageIds msgType)
        {
            return _lobbyLogFilters.TryGetValue(msgType, out var r) && r;
        }

        /// <summary>
        /// Whether or not the given Medius game client library message id should be logged
        /// </summary>
        public bool IsLog(MediusMGCLMessageIds msgType)
        {
            return _mgclLogFilters.TryGetValue(msgType, out var r) && r;
        }

        /// <summary>
        /// Whether or not the given Medius lobby extension message id should be logged
        /// </summary>
        public bool IsLog(MediusLobbyExtMessageIds msgType)
        {
            return _lobbyExtLogFilters.TryGetValue(msgType, out var r) && r;
        }

        /// <summary>
        /// Whether or not the given Medius Zipper plugin extension message id should be logged
        /// </summary>
        public bool IsLogPlugin(NetMessageTypeIds msgType)
        {
            return _netLogFilters.TryGetValue(msgType, out var r) && r;
        }

        /// <summary>
        /// Whether or not the given Medius GHS plugin extension message id should be logged
        /// </summary>
        public bool IsLogGHSPlugin(GhsOpcode msgType)
        {
            return _ghsOpsLogFilters.TryGetValue(msgType, out var r) && r;
        }

        /// <summary>
        /// Does some post processing on the deserialized model.
        /// </summary>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            // Load log filters in dictionary
            _rtLogFilters.Clear();
            if (RtLogFilter != null)
            {
                foreach (var filter in RtLogFilter)
                    if (Enum.TryParse<RT_MSG_TYPE>(filter, out var value))
                        _rtLogFilters.Add(value, true);
            }

            _dmeLogFilters.Clear();
            if (MediusDMEExtLogFilter != null)
            {
                foreach (var filter in MediusDMEExtLogFilter)
                    if (Enum.TryParse<MediusDmeMessageIds>(filter, out var value))
                        _dmeLogFilters.Add(value, true);
            }

            _lobbyLogFilters.Clear();
            if (MediusLobbyLogFilter != null)
            {
                foreach (var filter in MediusLobbyLogFilter)
                    if (Enum.TryParse<MediusLobbyMessageIds>(filter, out var value))
                        _lobbyLogFilters.Add(value, true);
            }

            _mgclLogFilters.Clear();
            if (MediusMGCLLogFilter != null)
            {
                foreach (var filter in MediusMGCLLogFilter)
                    if (Enum.TryParse<MediusMGCLMessageIds>(filter, out var value))
                        _mgclLogFilters.Add(value, true);
            }

            _lobbyExtLogFilters.Clear();
            if (MediusLobbyExtLogFilter != null)
            {
                foreach (var filter in MediusLobbyExtLogFilter)
                    if (Enum.TryParse<MediusLobbyExtMessageIds>(filter, out var value))
                        _lobbyExtLogFilters.Add(value, true);
            }
            /*
            _netLogFilters.Clear();
            if (MediusNetMessageTypesLogFilter != null)
            {
                foreach (var filter in MediusNetMessageTypesLogFilter)
                    if (Enum.TryParse<NetMessageTypeIds>(filter, out var value))
                        _netLogFilters.Add(value, true);
            }
            */

            _ghsOpsLogFilters.Clear();
            if (MediusGhsOpcodeFilter != null)
            {
                foreach (var filter in MediusGhsOpcodeFilter)
                    if (Enum.TryParse<GhsOpcode>(filter, out var value))
                        _ghsOpsLogFilters.Add(value, true);
            }
        }
    }
}
