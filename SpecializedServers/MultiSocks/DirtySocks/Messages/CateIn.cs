namespace MultiSocks.DirtySocks.Messages
{
    public class CateIn : AbstractMessage
    {
        public override string _Name { get => "cate"; }

        public string? VIEW { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new CateOut());
        }
    }
}
