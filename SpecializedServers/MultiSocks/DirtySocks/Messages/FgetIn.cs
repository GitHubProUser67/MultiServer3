namespace MultiSocks.DirtySocks.Messages
{
    public class FgetIn : AbstractMessage
    {
        public override string _Name { get => "fget"; }
        public string? TAG { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new FgetOut());
            client.SendMessage(new PlusFup());
        }
    }
}
