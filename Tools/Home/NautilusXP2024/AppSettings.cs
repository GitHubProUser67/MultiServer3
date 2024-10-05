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
        public string CdsEncryptOutputDirectory { get; set; }
        public string CdsDecryptOutputDirectory { get; set; }
        public string BarSdatSharcOutputDirectory { get; set; }
        public string MappedOutputDirectory { get; set; }
        public string HcdbOutputDirectory { get; set; }
        public string SqlOutputDirectory { get; set; }
        public string TicketListOutputDirectory { get; set; }
        public string LuacOutputDirectory { get; set; }
        public string LuaOutputDirectory { get; set; }
        public string AudioOutputDirectory { get; set; }

        public string RPCS3OutputDirectory { get; set; }

        public string VideoOutputDirectory { get; set; }
        public string ThemeColor { get; set; }
        public string PS3IPforFTP { get; set; }
        public string PS3TitleID { get; set; }

        public string SceneListSavePath { get; set; }

        public string SceneListPathURL { get; set; }

        public string TSSURL { get; set; }

        public string TSSeditorSavePath { get; set; }
        public OverwriteBehavior FileOverwriteBehavior { get; set; }
        public SaveDebugLog SaveDebugLogToggle { get; set; }
        public bool LiteCatalogueEnabled { get; set; }
        public ArchiveTypeSetting ArchiveTypeSettingRem { get; set; }
        public ArchiveMapperSetting ArchiveMapperSettingRem { get; set; }
        public RememberLastTabUsed LastTabUsed { get; set; }
        public bool IsOfflineMode { get; set; }


        // Constructor to initialize default values
        public AppSettings()
        {
            // Set default values
            CdsEncryptOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "CDSEncrypt");
            CdsDecryptOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "CDSDecrypt");
            BarSdatSharcOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "Archive");
            MappedOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "Mapped");
            HcdbOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "HCDB");
            SqlOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "SQL");
            TicketListOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "LST");
            LuacOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "LUAC");
            LuaOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "LUA");
            AudioOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "Audio");
            VideoOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "Video");
            RPCS3OutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "RPCS3");
            PS3IPforFTP = "192.168.199.121"; // Default IP address
            PS3TitleID = "PSHLQA186"; 
            SceneListSavePath = "C:\\PSMultiserver\\static\\wwwroot\\Environments\\"; 
            SceneListPathURL = "https://pshomeologylab.net/Environments/SceneList_dec.xml"; 
            TSSURL = "https://pshomeologylab.net/tss/coreHztFmpQrx0002_en-US.xml"; 
            TSSeditorSavePath = "C:\\PSMultiserver\\static\\wwwroot\\"; 
            ThemeColor = "#fc030f"; // Default color as a string
            SaveDebugLogToggle = SaveDebugLog.True;
            LiteCatalogueEnabled = true;
            ArchiveTypeSettingRem = ArchiveTypeSetting.SDAT;
            ArchiveMapperSettingRem = ArchiveMapperSetting.EXP;
            LastTabUsed = RememberLastTabUsed.ArchiveTool;
            IsOfflineMode = false; // Default to online mode
        }
    }


}
