namespace SRVEmu.Messages
{
    public class Addr : AbstractMessage
    {
        public override string _Name { get => "addr"; }

        public string? ADDR { get; set; }
        public string? PORT { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.Port = PORT;
        }
    }
}
