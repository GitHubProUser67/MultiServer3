namespace MultiSocks.Aries.SDK_v6.Messages.BurnoutParadisePlugin
{
    public class Opup : AbstractMessage
    {
        public override string _Name { get => "opup"; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            client.SendMessage(this);
        }
    }
}