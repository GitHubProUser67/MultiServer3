using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NautilusXP2024
{
    public class AppSettings
    {
        public string CdsOutputDirectory { get; set; }
        public string BarSdatSharcOutputDirectory { get; set; }
        public string MappedOutputDirectory { get; set; }
        public string HcdbOutputDirectory { get; set; }
        public string SqlOutputDirectory { get; set; }
        public string TicketListOutputDirectory { get; set; }
        public string LuacOutputDirectory { get; set; }
        public string LuaOutputDirectory { get; set; }
        public string InfToolOutputDirectory { get; set; }
        public string CacheOutputDirectory { get; set; }
        public int CpuPercentage { get; set; }
        public string ThemeColor { get; set; }
        public OverwriteBehavior FileOverwriteBehavior { get; set; }
        public SaveDebugLog SaveDebugLogToggle { get; set; }
        public ArchiveTypeSetting ArchiveTypeSettingRem { get; set; }
        public ArchiveMapperSetting ArchiveMapperSettingRem { get; set; }
        public RememberLastTabUsed LastTabUsed { get; set; }

        // Constructor to initialize default values
        public AppSettings()
        {
            // Set default values
            CdsOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "CDS");
            BarSdatSharcOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "Archive");
            MappedOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "Mapped");
            HcdbOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "HCDB");
            SqlOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "SQL");
            TicketListOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "LST");
            LuacOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "LUAC");
            LuaOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "LUA");
            InfToolOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "INF");
            CacheOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "Cache");
            CpuPercentage = 50;
            ThemeColor = "#FF00FF27"; // Default color as a string
            FileOverwriteBehavior = OverwriteBehavior.Rename;
            SaveDebugLogToggle = SaveDebugLog.True;
            ArchiveTypeSettingRem = ArchiveTypeSetting.SDAT;
            ArchiveMapperSettingRem = ArchiveMapperSetting.EXP;
            LastTabUsed = RememberLastTabUsed.ArchiveTool;
        }
    }


}
