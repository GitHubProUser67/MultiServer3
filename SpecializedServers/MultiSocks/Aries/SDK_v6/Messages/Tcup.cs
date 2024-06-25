namespace MultiSocks.Aries.SDK_v6.Messages
{
    public class Tcup : AbstractMessage
    {
        public override string _Name { get => "tcup"; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            client.SendMessage(this);
        }
    }
}
