namespace MultiSocks.DirtySocks.Messages
{
    public class PentIn : AbstractMessage
    {
        public override string _Name { get => "pent"; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new PentOut());
        }
    }
}
