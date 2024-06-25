namespace MultiSocks.Aries.SDK_v6.Messages
{
    public class Rent : AbstractMessage
    {
        public override string _Name { get => "rent"; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            client.SendMessage(this);
        }
    }
}
