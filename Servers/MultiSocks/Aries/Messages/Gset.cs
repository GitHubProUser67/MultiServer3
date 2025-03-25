using MultiSocks.Aries.Model;

namespace MultiSocks.Aries.Messages
{
    public class Gset : AbstractMessage
    {
        public override string _Name { get => "gset"; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            if (context is not MatchmakerServer mc) return;

            AriesUser? user = client.User;
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

            if (!string.IsNullOrEmpty(PERS))
            {
                AriesUser? otherUser = mc.Users.GetUserByPersonaName(PERS);

                if (otherUser != null && !string.IsNullOrEmpty(USERPARAMS))
                    otherUser.SetParametersFromString(USERPARAMS);
            }
            else if (!string.IsNullOrEmpty(USERPARAMS))
                user.SetParametersFromString(USERPARAMS);

            int? parsedMinSize = int.TryParse(GetInputCacheValue("MINSIZE"), out int minSize) ? minSize : null;
            int? parsedMaxSize = int.TryParse(GetInputCacheValue("MAXSIZE"), out int maxSize) ? maxSize : null;
            int? parsedPriv = int.TryParse(GetInputCacheValue("PRIV"), out int priv) ? priv : null;
            int? parsedRoomID = int.TryParse(GetInputCacheValue("ROOM"), out int room) ? room : null;
            int? parsedIdent = int.TryParse(GetInputCacheValue("IDENT"), out int ident) ? ident : null;

            if (user.CurrentGame != null)
            {
                // Check if any of the nullable variables are null before calling CreateGame
                if (parsedMinSize.HasValue && parsedMaxSize.HasValue && parsedRoomID.HasValue && parsedIdent.HasValue && !string.IsNullOrEmpty(CUSTFLAGS) &&
                    !string.IsNullOrEmpty(PARAMS) && !string.IsNullOrEmpty(NAME) && parsedPriv.HasValue &&
                    !string.IsNullOrEmpty(SEED))
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
                else if (!string.IsNullOrEmpty(FORCE_LEAVE) && FORCE_LEAVE == "1")
                {
                    AriesGame prevGame = user.CurrentGame;

                    if (prevGame.RemovePlayerByUsername(user.Username))
                    {
                        lock (mc.Games)
                            mc.Games.RemoveGame(prevGame);
                    }
                }
            }
            if (!string.IsNullOrEmpty(KICK) && user.CurrentGame != null)
            {
                foreach (string player in KICK.Split(','))
                {
                    if (user.CurrentGame.RemovePlayerByUsername(player, 1, KICK_REASON))
                    {
                        lock (mc.Games)
                            mc.Games.RemoveGame(user.CurrentGame);
                    }
                }
            }
            if (user.CurrentGame != null)
            {
                client.SendMessage(user.CurrentGame.GetGameDetails(_Name));

                if (!string.IsNullOrEmpty(SYSFLAGS))
                    user.CurrentGame.SysFlags = SYSFLAGS;

                user.CurrentGame.BroadcastPopulation(mc);
            }
        }
    }
}
