using MultiSocks.Aries.SDK_v1.Model;

namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Sviw : AbstractMessage
    {
        public override string _Name { get => "sviw"; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            string? VIEW = GetInputCacheValue("VIEW");

            if (context is not MatchmakerServerV1) return;

            User? user = client.User;
            if (user == null) return;

            if (VIEW == "DLC" || VIEW == "lobby")
            {
                client.SendMessage(new Dlc());
                client.SendMessage(new Ping());
            }
            else
            {
                // TODO, send dirtysocks error!
            }
        }
    }
}
