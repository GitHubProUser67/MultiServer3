namespace SRVEmu.DirtySocks.Messages
{
    public class PlusSes : AbstractMessage
    {
        public override string _Name { get => "+ses"; }

        public string? IDENT { get; set; }
        public string WHEN { get; set; } = "2003.12.8 15:52:54";
        public string NAME { get; set; } = "session";
        public string? HOST { get; set; } //host persona name
        public string? ROOM { get; set; } //room name
        public string COUNT { get; set; } = "2";
        public string USERFLAGS { get; set; } = "0";
        public string SYSFLAGS { get; set; } = "0";
        public string? KIND { get; set; }

        public string[]? OPID { get; set; } //opponent userid
        public string[]? OPPO { get; set; } //opponent persona
        public string[]? ADDR { get; set; } //ip for user

        public string? SEED { get; set; }
        public string? SELF { get; set; } //my persona name
    }
}
