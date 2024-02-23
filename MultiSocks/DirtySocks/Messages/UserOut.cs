namespace SRVEmu.DirtySocks.Messages
{
    public class UserOut : AbstractMessage
    {
        public override string _Name { get => "user"; }

        public string? PERS { get; set; }
        public string LAST { get; set; } = "2004.6.1 15:57:52";
        public string EXPR { get; set; } = "1072566000";
        public string STAT { get; set; } = string.Empty;
        public string CHEAT { get; set; } = "3";
        public string ACK_REP { get; set; } = "186";
        public string REP { get; set; } = "186";
        public string PLAST { get; set; } = "2004.6.1 15:57:46";
        public string PSINCE { get; set; } = "2003.11.25 07:56:09";
        public string DCNT { get; set; } = "0";
        public string? ADDR { get; set; }
        public string SERV { get; set; } = "159.153.229.239";
        public string RANK { get; set; } = "99999";
        public string? MESG { get; set; }
    }
}
