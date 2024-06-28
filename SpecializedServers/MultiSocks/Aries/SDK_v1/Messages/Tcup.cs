using MultiSocks.Aries.SDK_v6;
using MultiSocks.Aries.SDK_v6.Messages;

namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Tcup : AbstractMessage
    {
        public override string _Name { get => "tcup"; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            client.SendMessage(this);
        }
    }
}
