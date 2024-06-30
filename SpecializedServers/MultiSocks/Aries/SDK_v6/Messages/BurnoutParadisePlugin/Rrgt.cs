using MultiSocks.Aries.SDK_v6.Messages.BurnoutParadisePlugin.ErrorCodes;

namespace MultiSocks.Aries.SDK_v6.Messages.BurnoutParadisePlugin
{
    public class Rrgt : AbstractMessage
    {
        public override string _Name { get => "rrgt"; }
        public string? R { get; set; }
        public string? SET { get; set; }

        public override void Process(AbstractAriesServerV6 context, AriesClient client)
        {
            client.SendMessage(new RrgtTime());
        }
    }
}
