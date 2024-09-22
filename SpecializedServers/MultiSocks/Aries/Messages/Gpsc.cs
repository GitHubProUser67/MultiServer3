using MultiSocks.Aries.Model;

namespace MultiSocks.Aries.Messages
{
    public class Gpsc : AbstractMessage
    {
        public override string _Name { get => "gpsc"; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            if (context is not MatchmakerServer mc) return;

            AriesUser? user = client.User;
            if (user == null) return;

            string? CUSTFLAGS = GetInputCacheValue("CUSTFLAGS");
            string? NAME = GetInputCacheValue("NAME");
            string? PARAMS = GetInputCacheValue("PARAMS");
            string? PASS = GetInputCacheValue("PASS");
            string? SEED = GetInputCacheValue("SEED");
            string? SYSFLAGS = GetInputCacheValue("SYSFLAGS");
            string? FORCE_LEAVE = GetInputCacheValue("FORCE_LEAVE");
            string? USERPARAMS = GetInputCacheValue("USERPARAMS");

            if (!string.IsNullOrEmpty(USERPARAMS))
                user.SetParametersFromString(USERPARAMS);

            if (!string.IsNullOrEmpty(FORCE_LEAVE) && FORCE_LEAVE == "1" && user.CurrentGame != null)
            {
                AriesGame prevGame = user.CurrentGame;

                lock (mc.Games)
                {
                    if (prevGame.RemovePlayerByUsername(user.Username))
                        mc.Games.RemoveGame(prevGame);
                    else
                        mc.Games.UpdateGame(prevGame);
                }
            }

            int? parsedMinSize = int.TryParse(GetInputCacheValue("MINSIZE"), out int minSize) ? minSize : null;
            int? parsedMaxSize = int.TryParse(GetInputCacheValue("MAXSIZE"), out int maxSize) ? maxSize : null;
            int? parsedPriv = int.TryParse(GetInputCacheValue("PRIV"), out int priv) ? priv : null;

            // Check if any of the nullable variables are null before calling CreateGame
            if (parsedMinSize.HasValue && parsedMaxSize.HasValue && !string.IsNullOrEmpty(CUSTFLAGS) &&
                !string.IsNullOrEmpty(PARAMS) && !string.IsNullOrEmpty(NAME) && parsedPriv.HasValue &&
                !string.IsNullOrEmpty(SEED) && !string.IsNullOrEmpty(SYSFLAGS) && !string.IsNullOrEmpty(user.Username))
            {
                AriesGame? game = mc.Games.GetGameByName(NAME, PASS);

                game ??= mc.Games.AddGame(parsedMaxSize.Value, parsedMinSize.Value, CUSTFLAGS, PARAMS, NAME, parsedPriv.Value != 0, SEED, SYSFLAGS, PASS, 0);

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

                    client.SendMessage(this);

                    user.SendPlusWho(user, context.Project);

                    game.BroadcastPopulation(mc);
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
