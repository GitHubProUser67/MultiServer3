using MultiSocks.Aries.SDK_v6.Model;

namespace MultiSocks.Aries.SDK_v6.Messages
{
    public class Gdel : AbstractMessage
    {
        public override string _Name { get => "gdel"; }

        public override void Process(AbstractAriesServerV6 context, AriesClient client)
        {
            if (context is not MatchmakerServerV6 mc) return;

            User? user = client.User;
            if (user == null) return;

            if (user.CurrentGame != null)
            {
                string? FORCE = GetInputCacheValue("FORCE");

                if (!string.IsNullOrEmpty(FORCE) && FORCE == "1" || !user.CurrentGame.Started) // Don't quit immediatly if game is started or FORCE is triggered.
                {
                    lock (mc.Games)
                        mc.Games.RemoveGame(user.CurrentGame);
                }
                else
                {
                    // TODO, send DirtySocks Game already started error.
                }
            }
            else
            {
                // TODO, send DirtySocks Game not exist error.
            }
        }
    }
}
