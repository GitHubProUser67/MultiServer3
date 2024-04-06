namespace MultiSocks.DirtySocks.Messages
{
    public class UsldOut : AbstractMessage
    {
        public override string _Name { get => "usld"; }

        public string IMGATE { get; set; } = "0";
        public string QMSG0 { get; set; } = "Wanna play?";
        public string QMSG1 { get; set; } = "I rule!";
        public string QMSG2 { get; set; } = "Doh!";
        public string QMSG3 { get; set; } = "Mmmm... doughnuts.";
        public string QMSG04 { get; set; } = "What time is it?";
        public string QMSG05 { get; set; } = "The truth is out of style.";
        public string SPM_EA { get; set; } = "1";
        public string SPM_PART { get; set; } = "0";
        public string UID { get; set; } = "$00000000000003fe";
    }
}
