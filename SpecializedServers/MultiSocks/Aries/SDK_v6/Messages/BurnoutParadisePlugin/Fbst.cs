namespace MultiSocks.Aries.SDK_v6.Messages.BurnoutParadisePlugin
{
    public class Fbst : AbstractMessage
    {
        public override string _Name { get => "fbst"; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            client.SendMessage(this);
        }
    }
}