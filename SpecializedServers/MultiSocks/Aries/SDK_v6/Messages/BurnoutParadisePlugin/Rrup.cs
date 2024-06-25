using MultiSocks.Aries.SDK_v6.Messages.BurnoutParadisePlugin.ErrorCodes;

namespace MultiSocks.Aries.SDK_v6.Messages.BurnoutParadisePlugin
{
    public class Rrup : AbstractMessage
    {
        public override string _Name { get => "rrup"; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            client.SendMessage(new RrupTime());
        }
    }
}
