namespace SRVEmu.DirtySocks.Messages
{
    public class GseaOut : AbstractMessage
    {
        public override string _Name { get => "gsea"; }

        public string COUNT { get; set; } = "0";
    }
}
