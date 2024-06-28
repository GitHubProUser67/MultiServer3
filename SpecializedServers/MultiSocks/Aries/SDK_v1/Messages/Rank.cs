using MultiSocks.Aries.SDK_v1.Model;

namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Rank : AbstractMessage
    {
        public override string _Name { get => "rank"; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            if (context is not MatchmakerServerV1 mc) return;

            User? user = client.User;
            if (user == null) return;

            OutputCache.Add("RANK", "Unranked");
            OutputCache.Add("TIME", "1707089968");

            client.SendMessage(this);

            user.CurrentGame?.BroadcastPopulation(mc);

            user.CurrentGame?.SetGameStatus(false);
        }
    }
}
