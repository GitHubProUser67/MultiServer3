using MultiSocks.DirtySocks.Model;

namespace MultiSocks.DirtySocks.Messages
{
    public class GpscIn : AbstractMessage
    {
        public override string _Name { get => "gpsc"; }

        public string? CUSTFLAGS { get; set; }
        public string? MINSIZE { get; set; }
        public string? MAXSIZE { get; set; }
        public string? NAME { get; set; }
        public string? PARAMS { get; set; }
        public string? PASS { get; set; }
        public string? PRIV { get; set; }
        public string? SEED { get; set; }
        public string? SYSFLAGS { get; set; }
        public string? FORCE_LEAVE { get; set; }
        public string? USERPARAMS { get; set; }
        public string? REGIONS { get; set; }
        public string USERFLAGS { get; set; } = "0";

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            if (context is not MatchmakerServer mc) return;

            User? user = client.User;
            if (user == null) return;

            if (!string.IsNullOrEmpty(USERPARAMS))
                user.Params = USERPARAMS;

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

            int? parsedMinSize = int.TryParse(MINSIZE, out int minSize) ? minSize : null;
            int? parsedMaxSize = int.TryParse(MAXSIZE, out int maxSize) ? maxSize : null;
            int? parsedPriv = int.TryParse(PRIV, out int priv) ? priv : null;

            // Check if any of the nullable variables are null before calling CreateGame
            if (parsedMinSize.HasValue && parsedMaxSize.HasValue && !string.IsNullOrEmpty(CUSTFLAGS) &&
                !string.IsNullOrEmpty(PARAMS) && !string.IsNullOrEmpty(NAME) && parsedPriv.HasValue &&
                !string.IsNullOrEmpty(SEED) && !string.IsNullOrEmpty(SYSFLAGS) && !string.IsNullOrEmpty(user.Username))
            {
                Game? game = mc.Games.GetGameByName(NAME, PASS);

                game ??= mc.Games.AddGame(parsedMaxSize.Value, parsedMinSize.Value, CUSTFLAGS, PARAMS, NAME, parsedPriv.Value != 0, SEED, SYSFLAGS, PASS, (user.CurrentRoom != null) ? user.CurrentRoom.ID : 0);

                if (game != null)
                {
                    if (game.MinSize > 1) // Could it be more than 2?
                    {
                        if (game.Users.GetUserByName("@brobot24") == null)
                            game.AddHost(mc.Users.GetUserByName("@brobot24"));

                        if (game.Users.GetUserByName(user.Username) == null)
                            game.AddGPSHost(user);
                    }
                    else
                    {
                        if (game.Users.GetUserByName(user.Username) == null)
                            game.AddGPSHost(user);
                    }

                    user.CurrentGame = game;

                    client.SendMessage(new GpscOut());

                    if (!string.IsNullOrEmpty(context.Project))
                    {
                        if (context.Project.Contains("DPR-09"))
                            user.SendPlusWho(user, "DPR-09");
                        else if (context.Project.Contains("BURNOUT5"))
                            user.SendPlusWho(user, "BURNOUT5");
                    }
                    else
                        user.SendPlusWho(user, string.Empty);

                    game.BroadcastPopulation();
                }
                else
                {
                    // TODO SEND DIRTYSOCKS ERROR!
                }
            }
            else
            {
                // TODO SEND DIRTYSOCKS ERROR!
            }
        }
    }
}
