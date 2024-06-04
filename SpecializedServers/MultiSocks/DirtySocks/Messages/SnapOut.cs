namespace MultiSocks.DirtySocks.Messages
{
    public class SnapOut : AbstractMessage
    {
        public override string _Name { get => "snap"; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new SnapOut());
        }
    }
}
