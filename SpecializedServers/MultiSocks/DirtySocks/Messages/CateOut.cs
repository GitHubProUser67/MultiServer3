namespace MultiSocks.DirtySocks.Messages
{
    public class CateOut : AbstractMessage
    {
        public override string _Name { get => "cate"; }

        public string NSS { get; set; } = "18";
        public string SYMS { get; set; } = "TEST1,TEST2,TEST3";
        public string CC { get; set; } = "1";
        public string IC { get; set; } = "1";
        public string VC { get; set; } = "1";
        public string R { get; set; } = "0,1,1,1,2,0,0";
        public string U { get; set; } = "1,2";
    }
}
