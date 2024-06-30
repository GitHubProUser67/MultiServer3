namespace MultiSocks.Aries.SDK_v6.Messages.BurnoutParadisePlugin
{
    public class Rvup : AbstractMessage
    {
        public override string _Name { get => "rvup"; }

        public override void Process(AbstractAriesServerV6 context, AriesClient client)
        {
            client.SendMessage(this);
        }
    }
}