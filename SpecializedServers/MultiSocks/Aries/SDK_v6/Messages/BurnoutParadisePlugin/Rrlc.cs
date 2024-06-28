using MultiSocks.Aries.SDK_v6.Messages.BurnoutParadisePlugin.ErrorCodes;

namespace MultiSocks.Aries.SDK_v6.Messages.BurnoutParadisePlugin
{
    public class Rrlc : AbstractMessage
    {
        public override string _Name { get => "rrlc"; }

        public override void Process(AbstractAriesServerV6 context, AriesClient client)
        {
            client.SendMessage(new RrlcTime());
        }
    }
}
