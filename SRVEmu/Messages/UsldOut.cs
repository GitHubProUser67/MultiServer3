namespace SRVEmu.Messages
{
    public class UsldOut : AbstractMessage
    {
        public override string _Name { get => "usld"; }

        public string IMGATE { get; set; } = "0";
        public string QMSG0 { get; set; } = "TEST0";
        public string QMSG1 { get; set; } = "TEST1";
        public string QMSG2 { get; set; } = "TEST2";
        public string QMSG3 { get; set; } = "TEST3";
        public string QMSG04{ get; set; } = "TEST4";
        public string QMSG05 { get; set; } = "TEST5";
        public string SPM_EA { get; set; } = "0";
        public string SPM_PART { get; set; } = "0";
        public string UID { get; set; } = "$000000000b32588d";
    }
}
