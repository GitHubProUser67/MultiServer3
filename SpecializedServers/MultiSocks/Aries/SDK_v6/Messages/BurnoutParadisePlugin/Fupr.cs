namespace MultiSocks.Aries.SDK_v6.Messages.BurnoutParadisePlugin
{
    public class Fupr : AbstractMessage
    {
        public override string _Name { get => "fupr"; }

        public override void Process(AbstractAriesServerV6 context, AriesClient client)
        {
            client.SendMessage(this);
        }
    }
}
