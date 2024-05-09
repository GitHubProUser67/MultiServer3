namespace MultiSocks.DirtySocks.Messages
{
    public class RrlcIn : AbstractMessage
    {
        public override string _Name { get => "rrlc"; }
        public string? SKEY { get; set; }
        public string? NUM { get; set; }
        public string? IDX { get; set; }
        public string? SET { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new RrlcOut());
        }
    }
}
