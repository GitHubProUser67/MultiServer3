using MultiSocks.Aries.SDK_v6.Messages.ErrorCodes;
using MultiSocks.Aries.SDK_v6.Model;

namespace MultiSocks.Aries.SDK_v6.Messages
{
    public class Gqwk : AbstractMessage
    {
        public override string _Name { get => "gqwk"; }

        public override void Process(AbstractAriesServerV6 context, AriesClient client)
        {
            if (context is not MatchmakerServerV6 mc) return;

            User? user = client.User;
            if (user == null) return;

            string? USERPARAMS = GetInputCacheValue("USERPARAMS");
            string? FORCE_LEAVE = GetInputCacheValue("FORCE_LEAVE");

            if (!string.IsNullOrEmpty(USERPARAMS))
                user.SetParametersFromString(USERPARAMS);

            if (!string.IsNullOrEmpty(FORCE_LEAVE) && FORCE_LEAVE == "1" && user.CurrentGame != null)
            {
                Game prevGame = user.CurrentGame;

                lock (mc.Games)
                {
                    if (prevGame.RemovePlayerByUsername(user.Username))
                        mc.Games.RemoveGame(prevGame);
                    else
                        mc.Games.UpdateGame(prevGame);
                }
            }

            Game? game = mc.Games.GamesSessions.Values.Where(game => !game.Priv).FirstOrDefault();

            if (game != null)
            {
                game.AddUser(user);

                user.CurrentGame = game;

                client.SendMessage(game.GetGameDetails(_Name));

                user.SendPlusWho(user);

                game.BroadcastPopulation(mc);
            }
            else
                client.SendMessage(new MissOut(_Name));
        }
    }
}
