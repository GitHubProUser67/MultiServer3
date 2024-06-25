using MultiSocks.Aries.SDK_v6.Model;

namespace MultiSocks.Aries.SDK_v6.Messages
{
    public class Gdel : AbstractMessage
    {
        public override string _Name { get => "gdel"; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            if (context is not MatchmakerServer mc) return;

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
