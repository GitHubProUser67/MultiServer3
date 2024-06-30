namespace MultiSocks.Aries.SDK_v6.Messages
{
    public class Addr : AbstractMessage
    {
        public override string _Name { get => "addr"; }

        public string ADDR { get; set; } = "127.0.0.1";
        public string? PORT { get; set; }

        public override void Process(AbstractAriesServerV6 context, AriesClient client)
        {
            string? PORT = GetInputCacheValue("PORT");
            string? ADDR = GetInputCacheValue("ADDR");

            client.Port = PORT;
            client.LADDR = ADDR;

            client.SendMessage(new Ping());
        }
    }
}
