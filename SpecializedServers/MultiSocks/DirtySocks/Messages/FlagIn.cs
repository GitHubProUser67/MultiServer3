namespace MultiSocks.DirtySocks.Messages
{
    public class FlagIn : AbstractMessage
    {
        public override string _Name { get => "flag"; }
        public string? CLR { get; set; }
        public string? SET { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new FlagOut()
            {
                IDENT = "1",
                FLAGS = SET
            });
        }
    }
}
