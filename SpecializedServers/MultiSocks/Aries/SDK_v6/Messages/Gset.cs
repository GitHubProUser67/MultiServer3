using MultiSocks.Aries.SDK_v6.Model;

namespace MultiSocks.Aries.SDK_v6.Messages
{
    public class Gset : AbstractMessage
    {
        public override string _Name { get => "gset"; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            if (context is not MatchmakerServer mc) return;

            User? user = client.User;
            if (user == null) return;

            string? PERS = GetInputCacheValue("PERS");
            string? USERPARAMS = GetInputCacheValue("USERPARAMS");
            string? CUSTFLAGS = GetInputCacheValue("CUSTFLAGS");
            string? NAME = GetInputCacheValue("NAME");
            string? PARAMS = GetInputCacheValue("PARAMS");
            string? SEED = GetInputCacheValue("SEED");
            string? PASS = GetInputCacheValue("PASS");
            string? FORCE_LEAVE = GetInputCacheValue("FORCE_LEAVE");
            string? KICK = GetInputCacheValue("KICK");
            string? KICK_REASON = GetInputCacheValue("KICK_REASON");
            string? SYSFLAGS = GetInputCacheValue("SYSFLAGS");

            bool UpdatePlayerParams = false;

            if (!string.IsNullOrEmpty(PERS))
            {
                User? otherUser = mc.Users.GetUserByPersonaName(PERS);

                if (otherUser != null && !string.IsNullOrEmpty(USERPARAMS))
                {
                    otherUser.SetParametersFromString(USERPARAMS);
                    otherUser.CurrentGame?.UpdatePlayerParams(otherUser);
                }
            }
            else if (!string.IsNullOrEmpty(USERPARAMS))
            {
                UpdatePlayerParams = true;
                user.SetParametersFromString(USERPARAMS);
            }

            int? parsedMinSize = int.TryParse(GetInputCacheValue("MINSIZE"), out int minSize) ? minSize : null;
            int? parsedMaxSize = int.TryParse(GetInputCacheValue("MAXSIZE"), out int maxSize) ? maxSize : null;
            int? parsedPriv = int.TryParse(GetInputCacheValue("PRIV"), out int priv) ? priv : null;
            int? parsedRoomID = int.TryParse(GetInputCacheValue("ROOM"), out int room) ? room : null;
            int? parsedIdent = int.TryParse(GetInputCacheValue("IDENT"), out int ident) ? ident : null;

            // Check if any of the nullable variables are null before calling CreateGame
            if (parsedMinSize.HasValue && parsedMaxSize.HasValue && parsedRoomID.HasValue && parsedIdent.HasValue && !string.IsNullOrEmpty(CUSTFLAGS) &&
                !string.IsNullOrEmpty(PARAMS) && !string.IsNullOrEmpty(NAME) && parsedPriv.HasValue &&
                !string.IsNullOrEmpty(SEED) && !string.IsNullOrEmpty(user.Username) && user.CurrentGame != null)
            {
                user.CurrentGame.Name = NAME;
                user.CurrentGame.pass = PASS;
                user.CurrentGame.MinSize = parsedMinSize.Value;
                user.CurrentGame.MaxSize = parsedMaxSize.Value;
                user.CurrentGame.CustFlags = CUSTFLAGS;
                user.CurrentGame.RoomID = parsedRoomID.Value;
                user.CurrentGame.ID = parsedIdent.Value;
                user.CurrentGame.Priv = parsedPriv.Value == 1;
                user.CurrentGame.Seed = SEED;
                user.CurrentGame.Params = PARAMS;
            }
            else if (!string.IsNullOrEmpty(FORCE_LEAVE) && FORCE_LEAVE == "1" && user.CurrentGame != null)
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

            if (!string.IsNullOrEmpty(KICK))
            {
                foreach (string player in KICK.Split(','))
                {
                    lock (mc.Games)
                    {
                        if (user.CurrentGame != null)
                        {
                            if (user.CurrentGame.RemovePlayerByUsername(player, 1, KICK_REASON))
                                mc.Games.RemoveGame(user.CurrentGame);
                            else
                                mc.Games.UpdateGame(user.CurrentGame);
                        }
                    }
                }
            }

            if (user.CurrentGame != null)
            {
                client.SendMessage(user.CurrentGame.GetGameDetails(_Name));

                if (!string.IsNullOrEmpty(SYSFLAGS))
                    user.CurrentGame.SysFlags = SYSFLAGS;

                if (UpdatePlayerParams)
                    user.CurrentGame.UpdatePlayerParams(user);

                lock (mc.Games)
                    mc.Games.UpdateGame(user.CurrentGame);

                user.CurrentGame.BroadcastPopulation(mc);
            }
            else
            {
                // TODO SEND DIRTYSOCKS ERROR!
            }
        }
    }
}
