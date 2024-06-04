namespace MultiSocks.DirtySocks.Messages
{
    public class RrgtIn : AbstractMessage
    {
        public override string _Name { get => "rrgt"; }
        public string? R { get; set; }
        public string? SET { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new RrgtOut());
        }
    }
}
