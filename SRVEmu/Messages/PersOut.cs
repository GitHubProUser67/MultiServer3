namespace SRVEmu.Messages
{
    public class PersOut : AbstractMessage
    {
        public override string _Name { get => "pers"; }

        public string? NAME { get; set; }
        public string? PERS { get; set; }
        public string LAST { get; set; } = "2003.12.8 15:51:58";
        public string PLAST { get; set; } = "2003.12.8 16:51:40";
        public string LKEY { get; set; } = "3fcf27540c92935b0a66fd3b0000283c";
    }
}
