namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Rent : AbstractMessage
    {
        public override string _Name { get => "rent"; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            client.SendMessage(this);
        }
    }
}
