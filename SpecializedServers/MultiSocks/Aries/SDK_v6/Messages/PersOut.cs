namespace MultiSocks.Aries.SDK_v6.Messages
{
    public class PersOut : AbstractMessage
    {
        public override string _Name { get => "pers"; }

        public string? A { get; set; }
        public string? IDLE { get; set; }
        public string? LA { get; set; }
        public string? LOC { get; set; }
        public string? MA { get; set; }
        public string? PSINCE { get; set; }
        public string? SINCE { get; set; }
        public string? NAME { get; set; }
        public string? PERS { get; set; }
        public string? STAT { get; set; }
        public string? LAST { get; set; }
        public string? PLAST { get; set; }
        public string? LKEY { get; set; }
    }
}
