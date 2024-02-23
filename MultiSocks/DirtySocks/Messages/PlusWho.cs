namespace SRVEmu.DirtySocks.Messages
{
    public class PlusWho : AbstractMessage
    {
        public override string _Name { get => "+who"; }
        public string? M { get; set; }
        public string? N { get; set; }
        public string? MA { get; set; }
        public string? A { get; set; }
        public string? LA { get; set; }
        public string? P { get; set; }
        public string? CL { get; set; }
        public string F { get; set; } = string.Empty;
        public string? G { get; set; }
        public string? HW { get; set; }
        public string? I { get; set; }
        public string? LO { get; set; }
        public string? LV { get; set; }
        public string? MD { get; set; }
        public string? PRES { get; set; }
        public string? RP { get; set; }
        public string S { get; set; } = string.Empty;
        public string? AT { get; set; }
        public string? C { get; set; }
        public string? US { get; set; }
        public string? VERS { get; set; }
        public string? X { get; set; }
        public string? R { get; set; } //room
        public string? RI { get; set; } //room id
        public string RF { get; set; } = "C";
        public string RT { get; set; } = "1";
    }
}
