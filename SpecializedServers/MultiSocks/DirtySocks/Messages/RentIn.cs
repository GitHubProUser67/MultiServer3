namespace MultiSocks.DirtySocks.Messages
{
    public class RentIn : AbstractMessage
    {
        public override string _Name { get => "rent"; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new RentOut());
        }
    }
}
