namespace SRVEmu.Messages
{
    public class SeleOut : AbstractMessage
    {
        public override string _Name { get => "sele"; }

        public string GAMES { get; set; } = "1";
        public string ROOMS { get; set; } = "1";
        public string USERS { get; set; } = "1";
        public string MESGS { get; set; } = "1";
        public string RANKS { get; set; } = "0";
        public string MORE { get; set; } = "1";
        public string SLOTS { get; set; } = "36";
    }
}
