namespace SRVEmu.DirtySocks.Messages
{
    public class DperOut : AbstractMessage
    {
        public override string _Name { get => "dper"; }

        public string? NAME { get; set; }
        public string? PERS { get; set; }
    }
}
