using MultiSocks.Aries.SDK_v6.Model;

namespace MultiSocks.Aries.SDK_v6.Messages
{
    public class Sviw : AbstractMessage
    {
        public override string _Name { get => "sviw"; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            string? VIEW = GetInputCacheValue("VIEW");

            if (context is not MatchmakerServer) return;

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
