namespace SRVEmu.DirtySocks.Messages
{
    public class CperOut : AbstractMessage
    {
        public override string _Name { get => "cper"; }

        public string? NAME { get; set; }
        public string? PERS { get; set; }
    }
}
