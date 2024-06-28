using MultiSocks.Aries.SDK_v1.Model;

namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Gsta : AbstractMessage
    {
        public override string _Name { get => "gsta"; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            if (context is not MatchmakerServerV1 mc) return;

            User? user = client.User;
            if (user == null) return;

            if (user.CurrentGame != null)
            {
                user.CurrentGame.SetGameStatus(true);

                lock (mc.Games)
                    mc.Games.UpdateGame(user.CurrentGame);

                user.Connection?.SendMessage(this);

                user.SendPlusWho(user);

                user.CurrentGame.BroadcastPopulation(mc);

                user.CurrentGame.BroadcastPlusSes();
            }
            else
            {
                // TODO SEND DIRTYSOCKS ERROR!
            }
        }
    }
}
