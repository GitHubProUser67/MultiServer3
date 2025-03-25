using MultiSocks.Aries.Model;

namespace MultiSocks.Aries.Messages
{
    public class Gjoi : AbstractMessage
    {
        public override string _Name { get => "gjoi"; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            if (context is not MatchmakerServer mc) return;

            AriesUser? user = client.User;
            if (user == null) return;

            string? FORCE_LEAVE = GetInputCacheValue("FORCE_LEAVE");
            string? SESS = GetInputCacheValue("SESS");
            string? SEED = GetInputCacheValue("SEED");
            string? PASS = GetInputCacheValue("PASS");
            string? CUSTFLAGS = GetInputCacheValue("CUSTFLAGS");
            string? NAME = GetInputCacheValue("NAME");
            string? PARAMS = GetInputCacheValue("PARAMS");
            string? SYSFLAGS = GetInputCacheValue("SYSFLAGS");

            if (!string.IsNullOrEmpty(FORCE_LEAVE) && FORCE_LEAVE == "1" && user.CurrentGame != null)
            {
                AriesGame prevGame = user.CurrentGame;

                if (prevGame.RemovePlayerByUsername(user.Username))
                {
                    lock (mc.Games)
                        mc.Games.RemoveGame(prevGame);
                }
            }

            int? parsedMinSize = int.TryParse(GetInputCacheValue("MINSIZE"), out int minSize) ? minSize : null;
            int? parsedMaxSize = int.TryParse(GetInputCacheValue("MAXSIZE"), out int maxSize) ? maxSize : null;
            int? parsedPriv = int.TryParse(GetInputCacheValue("PRIV"), out int priv) ? priv : null;
            int? parsedRoomID = int.TryParse(GetInputCacheValue("ROOM"), out int room) ? room : null;
            int? parsedIdent = int.TryParse(GetInputCacheValue("IDENT"), out int ident) ? ident : null;

            if (!string.IsNullOrEmpty(SESS) && SESS.Equals("Invite") && parsedMinSize.HasValue && parsedMaxSize.HasValue
                && parsedIdent.HasValue && parsedPriv.HasValue && !string.IsNullOrEmpty(SEED))
            {
                AriesGame? game = mc.Games.GamesSessions.Values.Where(game => game.pass == PASS && /*game.MinSize == parsedMinSize.Value 
                && game.MaxSize == parsedMaxSize.Value Commented out, Burnout has an offset for some WEIRD reasons...  */ game.ID == parsedIdent.Value && game.Priv == (parsedPriv.Value == 1) && game.Seed == SEED).FirstOrDefault();

                if (game != null)
                {
                    game.AddUser(user);

                    user.CurrentGame = game;

                    client.SendMessage(game.GetGameDetails("gjoi"));

                    user.SendPlusWho(user, context.Project);

                    game.BroadcastPopulation(mc);

                    return;
                }
            }

            // Check if any of the nullable variables are null before calling CreateGame
            else if (parsedMinSize.HasValue && parsedMaxSize.HasValue && parsedRoomID.HasValue && parsedIdent.HasValue && !string.IsNullOrEmpty(CUSTFLAGS) &&
                !string.IsNullOrEmpty(PARAMS) && !string.IsNullOrEmpty(NAME) && parsedPriv.HasValue &&
                !string.IsNullOrEmpty(SEED) && !string.IsNullOrEmpty(SYSFLAGS))
            {
                AriesGame? game = mc.Games.GamesSessions.Values.Where(game => game.Name.Equals(NAME) && game.pass == PASS
                && game.MinSize == parsedMinSize.Value && game.MaxSize == parsedMaxSize.Value && game.CustFlags == CUSTFLAGS
                && game.SysFlags == SYSFLAGS && game.RoomID == parsedRoomID.Value && game.ID == parsedIdent.Value && game.Priv == (parsedPriv.Value == 1) && game.Seed == SEED).FirstOrDefault();

                if (game != null)
                {
                    game.AddUser(user);

                    user.CurrentGame = game;

                    client.SendMessage(game.GetGameDetails(_Name));

                    user.SendPlusWho(user, context.Project);

                    game.BroadcastPopulation(mc);

                    return;
                }
            }

            client.SendMessage(new GjoiUgam());
        }
    }
}
