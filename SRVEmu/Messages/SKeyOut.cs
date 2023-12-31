namespace SRVEmu.Messages
{
    public class SKeyOut : AbstractMessage
    {
        public override string _Name { get => "skey"; }

        public string SKEY { get; set; } = "$37940faf2a8d1381a3b7d0d2f570e6a7";
        public string? DP { get; set; }
        public string? GFID { get; set; }
        public string? PLATFORM { get; set; }
        public string? PSID { get; set; }

    }
}
