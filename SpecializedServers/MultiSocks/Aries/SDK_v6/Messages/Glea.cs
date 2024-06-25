using MultiSocks.Aries.SDK_v6.Model;

namespace MultiSocks.Aries.SDK_v6.Messages
{
    public class Glea : AbstractMessage
    {
        public override string _Name { get => "glea"; }

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
                    client.SendMessage(user.CurrentGame.GetGameDetails(_Name));

                    Game prevGame = user.CurrentGame;

                    lock (mc.Games)
                    {
                        if (prevGame.RemovePlayerByUsername(user.Username))
                            mc.Games.RemoveGame(prevGame);
                        else
                            mc.Games.UpdateGame(prevGame);
                    }
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
