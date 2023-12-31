namespace SRVEmu.Messages
{
    public class Addr : AbstractMessage
    {
        public override string _Name { get => "addr"; }

        public string ADDR { get; set; } = "127.0.0.1";
        public string? PORT { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.Port = PORT;
            client.IP = ADDR;
        }
    }
}
