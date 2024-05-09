namespace MultiSocks.DirtySocks.Messages
{
    public class RvupIn : AbstractMessage
    {
        public override string _Name { get => "rvup"; }
        public string? RIVAL0 { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new RvupOut());
        }
    }
}
