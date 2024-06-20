using MultiSocks.DirtySocks.Model;

namespace MultiSocks.DirtySocks.Messages
{
    public class GsetIn : AbstractMessage
    {
        public override string _Name { get => "gset"; }

        public string? NAME { get; set; }
        public string? PASS { get; set; }
        public string? PARAMS { get; set; }
        public string? MINSIZE { get; set; }
        public string? MAXSIZE { get; set; }
        public string? CUSTFLAGS { get; set; }
        public string? SYSFLAGS { get; set; }
        public string? ROOM { get; set; }
        public string? IDENT { get; set; }
        public string? SESS { get; set; }
        public string? PRIV { get; set; }
        public string? SEED { get; set; }
        public string? FORCE_LEAVE { get; set; }
        public string? KICK { get; set; }
        public string? KICK_REASON { get; set; }
        public string? KICK_SET { get; set; }
        public string? USERPARAMS { get; set; }
        public string? PERS { get; set; }
        public string? USERFLAGS { get; set; } = "0";

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            if (context is not MatchmakerServer mc) return;

            User? user = client.User;
            if (user == null) return;

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

            int? parsedMinSize = int.TryParse(MINSIZE, out int minSize) ? minSize : null;
            int? parsedMaxSize = int.TryParse(MAXSIZE, out int maxSize) ? maxSize : null;
            int? parsedPriv = int.TryParse(PRIV, out int priv) ? priv : null;
            int? parsedRoomID = int.TryParse(ROOM, out int room) ? room : null;
            int? parsedIdent = int.TryParse(IDENT, out int ident) ? ident : null;

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
                client.SendMessage(user.CurrentGame.GetGsetOut());

                if (!string.IsNullOrEmpty(SYSFLAGS))
                    user.CurrentGame.SysFlags = SYSFLAGS;

                if (UpdatePlayerParams)
                    user.CurrentGame.UpdatePlayerParams(user);

                lock (mc.Games)
                    mc.Games.UpdateGame(user.CurrentGame);

                user.CurrentGame.BroadcastPopulation();
            }
            else
            {
                // TODO SEND DIRTYSOCKS ERROR!
            }
        }
    }
}
