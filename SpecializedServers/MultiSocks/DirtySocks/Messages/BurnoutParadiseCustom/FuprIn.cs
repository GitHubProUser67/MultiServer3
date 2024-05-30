namespace MultiSocks.DirtySocks.Messages
{
    public class FuprIn : AbstractMessage
    {
        public override string _Name { get => "fupr"; }

        public string? PRES { get; set; }
        public string? JOIN { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new FuprOut());
        }
    }
}
