namespace SRVEmu.Messages
{
    class PlusWho : AbstractMessage
    {
        public override string _Name { get => "+who"; }
        public string? M { get; set; }
        public string? N { get; set; }
        public string MA { get; set; } = string.Empty;
        public string A { get; set; } = "24.141.39.62";
        public string LA { get; set; } = "192.168.0.133";
        public string P { get; set; } = "1";
        public string CL { get; set; } = "511";
        public string F { get; set; } = "U";
        public string G { get; set; } = "0";
        public string HW { get; set; } = "0";
        public string I { get; set; } = "71615";
        public string LO { get; set; } = "enUS";
        public string LV { get; set; } = "1049601";
        public string MD { get; set; } = "0";
        public string PRES { get; set; } = string.Empty;
        public string RP { get; set; } = "0";
        public string S { get; set; } = string.Empty;
        public string US { get; set; } = "0";
        public string VERS { get; set; } = "5";
        public string? X { get; set; } = string.Empty;
        public string? R { get; set; } //room
        public string? RI { get; set; } //room id
        public string RF { get; set; } = "C";
        public string RT { get; set; } = "1";
    }
}
