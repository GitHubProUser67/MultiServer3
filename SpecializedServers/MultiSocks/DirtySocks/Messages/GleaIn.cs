using MultiSocks.DirtySocks.Model;

namespace MultiSocks.DirtySocks.Messages
{
    public class GleaIn : AbstractMessage
    {
        public override string _Name { get => "glea"; }
        public string? SET { get; set; }
        public string? FORCE { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            if (context is not MatchmakerServer mc) return;

            User? user = client.User;
            if (user == null) return;

            if (user.CurrentGame != null)
            {
                if ((!string.IsNullOrEmpty(FORCE) && FORCE == "1") || !user.CurrentGame.Started) // Don't quit immediatly if game is started or FORCE is triggered.
                {
                    client.SendMessage(user.CurrentGame.GetGleaOut());

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
