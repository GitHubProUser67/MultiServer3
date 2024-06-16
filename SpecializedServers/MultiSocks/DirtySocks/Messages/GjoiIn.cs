using MultiSocks.DirtySocks.Model;

namespace MultiSocks.DirtySocks.Messages
{
    public class GjoiIn : AbstractMessage
    {
        public override string _Name { get => "gjoi"; }

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
        public string? USERPARAMS { get; set; }
        public string USERFLAGS { get; set; } = "0";

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            if (context is not MatchmakerServer mc) return;

            User? user = client.User;
            if (user == null) return;

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
            int? parsedRoomID = int.TryParse(ROOM, out int room) ? room : null;
            int? parsedIdent = int.TryParse(IDENT, out int ident) ? ident : null;

            if (!string.IsNullOrEmpty(SESS) && SESS.Equals("Invite") && parsedMinSize.HasValue && parsedMaxSize.HasValue
                && parsedIdent.HasValue && parsedPriv.HasValue && !string.IsNullOrEmpty(SEED) && !string.IsNullOrEmpty(user.Username))
            {
                Game? game = mc.Games.GamesSessions.Values.Where(game => game.pass == PASS && /*game.MinSize == parsedMinSize.Value 
                && game.MaxSize == parsedMaxSize.Value Commented out, Burnout has an offset for some WEIRD reasons...  */ game.ID == parsedIdent.Value && (game.Priv == (parsedPriv.Value == 1)) && game.Seed == SEED).FirstOrDefault();

                if (game != null)
                {
                    game.AddUser(user);

                    user.CurrentGame = game;

                    client.SendMessage(game.GetGjoiOut());

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

            // Check if any of the nullable variables are null before calling CreateGame
            else if (parsedMinSize.HasValue && parsedMaxSize.HasValue && parsedRoomID.HasValue && parsedIdent.HasValue && !string.IsNullOrEmpty(CUSTFLAGS) &&
                !string.IsNullOrEmpty(PARAMS) && !string.IsNullOrEmpty(NAME) && parsedPriv.HasValue &&
                !string.IsNullOrEmpty(SEED) && !string.IsNullOrEmpty(SYSFLAGS) && !string.IsNullOrEmpty(user.Username))
            {
                Game? game = mc.Games.GamesSessions.Values.Where(game => game.Name.Equals(NAME) && game.pass == PASS
                && game.MinSize == parsedMinSize.Value && game.MaxSize == parsedMaxSize.Value && game.CustFlags == CUSTFLAGS
                && game.SysFlags == SYSFLAGS && game.RoomID == parsedRoomID.Value && game.ID == parsedIdent.Value && (game.Priv == (parsedPriv.Value == 1)) && game.Seed == SEED).FirstOrDefault();

                if (game != null)
                {
                    game.AddUser(user);

                    user.CurrentGame = game;

                    client.SendMessage(game.GetGjoiOut());

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
