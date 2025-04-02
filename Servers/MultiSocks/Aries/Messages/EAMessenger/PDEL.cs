namespace MultiSocks.Aries.Messages
{
    public class PDEL : AbstractMessage
    {
        public override string _Name { get => "PDEL"; }
        public string? USER { get; set; }
        public string? RSRC { get; set; }
        public string? LSRC { get; set; }
        public string? DOMN { get; set; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            PDEL pdel = new PDEL();

            client.SendMessage(pdel);
        }
    }
}