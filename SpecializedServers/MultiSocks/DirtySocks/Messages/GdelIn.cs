using MultiSocks.DirtySocks.Model;

namespace MultiSocks.DirtySocks.Messages
{
    public class GdelIn : AbstractMessage
    {
        public override string _Name { get => "gdel"; }
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
