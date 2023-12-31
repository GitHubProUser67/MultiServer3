namespace SRVEmu.Messages
{
    public class SeleOut : AbstractMessage
    {
        public override string _Name { get => "sele"; }
        public string DP { get; set; } = "PC/Burnout-2008/na1";
        public string ASYNC { get; set; } = "0";
        public string CTRL { get; set; } = "0";
        public string GAMES { get; set; } = "0";
        public string GFID { get; set; } = "\"ODS:19038.110.Base Product;BURNOUT PARADISE ULTIMATE EDITION_PC_ONLINE_ACCESS\"";
        public string INGAME { get; set; } = "0";
        public string ROOMS { get; set; } = "0";
        public string USERS { get; set; } = "0";
        public string USERSETS { get; set; } = "0";
        public string MESGS { get; set; } = "0";
        public string MESGTYPES { get; set; } = string.Empty;
        public string MYGAME { get; set; } = "1";
        public string PLATFORM { get; set; } = "pc";
        public string PSID { get; set; } = "PS-REG-BURNOUT2008";
        public string RANKS { get; set; } = "0";
        public string MORE { get; set; } = "1";
        public string SLOTS { get; set; } = "280";
        public string STATS { get; set; } = "0";
    }
}
