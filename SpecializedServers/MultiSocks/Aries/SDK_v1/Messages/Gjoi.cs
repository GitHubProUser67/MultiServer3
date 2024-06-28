using MultiSocks.Aries.SDK_v1.Model;

namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Gjoi : AbstractMessage
    {
        public override string _Name { get => "gjoi"; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            if (context is not MatchmakerServerV1 mc) return;

            User? user = client.User;
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
                Game prevGame = user.CurrentGame;

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
            int? parsedRoomID = int.TryParse(GetInputCacheValue("ROOM"), out int room) ? room : null;
            int? parsedIdent = int.TryParse(GetInputCacheValue("IDENT"), out int ident) ? ident : null;

            if (!string.IsNullOrEmpty(SESS) && SESS.Equals("Invite") && parsedMinSize.HasValue && parsedMaxSize.HasValue
                && parsedIdent.HasValue && parsedPriv.HasValue && !string.IsNullOrEmpty(SEED) && !string.IsNullOrEmpty(user.Username))
            {
                Game? game = mc.Games.GamesSessions.Values.Where(game => game.pass == PASS && /*game.MinSize == parsedMinSize.Value 
                && game.MaxSize == parsedMaxSize.Value Commented out, Burnout has an offset for some WEIRD reasons...  */ game.ID == parsedIdent.Value && game.Priv == (parsedPriv.Value == 1) && game.Seed == SEED).FirstOrDefault();

                if (game != null)
                {
                    game.AddUser(user);

                    user.CurrentGame = game;

                    client.SendMessage(game.GetGameDetails("gjoi"));

                    user.SendPlusWho(user);

                    game.BroadcastPopulation(mc);
                }
                else
                {
                    // TODO SEND DIRTYSOCKS ERROR!
                }
            }

            // Check if any of the nullable variables are null before calling CreateGame
            else if (parsedMinSize.HasValue && parsedMaxSize.HasValue && parsedRoomID.HasValue && parsedIdent.HasValue && !string.IsNullOrEmpty(CUSTFLAGS) &&
                !string.IsNullOrEmpty(PARAMS) && !string.IsNullOrEmpty(NAME) && parsedPriv.HasValue &&
                !string.IsNullOrEmpty(SEED) && !string.IsNullOrEmpty(SYSFLAGS) && !string.IsNullOrEmpty(user.Username))
            {
                Game? game = mc.Games.GamesSessions.Values.Where(game => game.Name.Equals(NAME) && game.pass == PASS
                && game.MinSize == parsedMinSize.Value && game.MaxSize == parsedMaxSize.Value && game.CustFlags == CUSTFLAGS
                && game.SysFlags == SYSFLAGS && game.RoomID == parsedRoomID.Value && game.ID == parsedIdent.Value && game.Priv == (parsedPriv.Value == 1) && game.Seed == SEED).FirstOrDefault();

                if (game != null)
                {
                    game.AddUser(user);

                    user.CurrentGame = game;

                    client.SendMessage(game.GetGameDetails(_Name));

                    user.SendPlusWho(user);

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
