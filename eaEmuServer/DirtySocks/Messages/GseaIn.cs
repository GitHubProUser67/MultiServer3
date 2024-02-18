namespace SRVEmu.DirtySocks.Messages
{
    public class GseaIn : AbstractMessage
    {
        public override string _Name { get => "gsea"; }
        public string? START { get; set; }
        public string? COUNT { get; set; }
        public string? ASYNC { get; set; }
        public string? SYSMASK { get; set; }
        public string? SYSFLAGS { get; set; }
        public string? PLAYERS { get; set; }
        public string? CUSTOM { get; set; }
        public string? CUSTMASK { get; set; }
        public string? CUSTFLAGS { get; set; }
        public string? CANCEL { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new GseaOut()
            {

            });
        }
    }
}
