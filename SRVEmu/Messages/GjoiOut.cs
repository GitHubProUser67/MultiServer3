namespace SRVEmu.Messages
{
    public class GjoiOut : AbstractMessage
    {
        public override string _Name { get => "gjoi"; }

        public string TI { get; set; } = "1001";
        public string N { get; set; } = "room";
        public string? H { get; set; }
        public string D { get; set; } = "burnout revival";
        public string F { get; set; } = "CK";
        public string A { get; set; } = "24.141.39.62";
        public string T { get; set; } = "0";
        public string L { get; set; } = "5";
        public string P { get; set; } = "0";
    }
}
