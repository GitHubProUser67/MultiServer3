namespace SRVEmu.Messages
{
    public class SdtaIn : AbstractMessage
    {
        public override string _Name { get => "sdta"; }
        public string? PERS { get; set; }
        public string? SLOT { get; set; }
        public string? VIEW { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new SdtaOut());
        }
    }
}
