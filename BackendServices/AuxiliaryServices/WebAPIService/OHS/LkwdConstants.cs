using System.Collections.Generic;

namespace WebAPIService.OHS
{
    public class LkwdConstants
    {
        public static readonly Dictionary<string, ushort> TokensUUIDs = new Dictionary<string, ushort>
        {
            { "3CE5F061-126E455D-81BE66A9-A5FFE0DD", 61},
            { "41572A7D-7472434F-A9CC5E74-1C46EC72", 6240},
            { "558EDF38-5064415B-9D523BB1-E4826C88", 480},
            { "5B9BCC58-E794496A-B10A027C-DAFB1C19", 80 },
            { "DA1418BE-3B5B4493-BF2A2FDC-FF4E7EBD", 2780 },
            { "FAE204D3-40CA42AA-8055A1D3-8721EE08", 1180 }
        };

        public static List<string> LockwoodLifeSceneList = new List<string>
        {
            "Dream_Yacht_Club_946D_7710",
            "Sunset_Lounge_EBEF_AD7C",
            "Dream_Forest_Apt_82AC_F9C3",
            "Ajagaras_Peak_Public_211C_B987",
            "Dream_Yacht_Arc_Club_9700_7EBC",
            "FortBellamyPublic_2A32_1E14",
            "Dream_Hideaway_Club_FB13_AB2E",
            "ArcticDreamYacht_Apt_D1C5_F674",
            "Castle_Strakh_CFDB_36FE",
            "Millionaires_Hub_1508_98C7",
            "Millionaires_Yacht_CC55_902E",
            "Haunted_Castle_E770_BD3B",
            "Dream_Island_Club_E2C0_8735",
            "DreamAdventureApt_C0D0_CDB0",
            "Ajagaras_Peak_Apt_4A94_3E08",
            "Millionaires_Beach_BC6F_6E33",
            "Dream_Forest_Club_6A43_ADD6",
            "Dream_Hideaway_Apt_577C_70EE"
        };
    }
}
