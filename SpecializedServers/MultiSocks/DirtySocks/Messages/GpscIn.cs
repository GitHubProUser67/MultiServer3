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

            int? parsedMinSize = int.TryParse(MINSIZE, out int minSize) ? minSize : null;
            int? parsedMaxSize = int.TryParse(MAXSIZE, out int maxSize) ? maxSize : null;
            int? parsedPriv = int.TryParse(PRIV, out int priv) ? priv : null;

            // Check if any of the nullable variables are null before calling CreateGame
            if (parsedMinSize.HasValue && parsedMaxSize.HasValue && !string.IsNullOrEmpty(CUSTFLAGS) &&
                !string.IsNullOrEmpty(PARAMS) && !string.IsNullOrEmpty(NAME) && parsedPriv.HasValue &&
                !string.IsNullOrEmpty(SEED) && !string.IsNullOrEmpty(SYSFLAGS) && !string.IsNullOrEmpty(user.Username))
            {
                Game? game = mc.Games.GetGameByName(NAME, PASS);

                game ??= mc.Games.AddGame(parsedMinSize.Value, parsedMaxSize.Value, CUSTFLAGS, PARAMS, NAME, parsedPriv.Value != 0, SEED, SYSFLAGS, PASS);

                if (game != null)
                {
                    if (game.Users.GetUserByName(user.Username) == null)
                        game.Users.AddUser(user);

                    if (game.Users.Count() < game.MinSize) // If we are still bellow minimum player count, we add the admin player.
                        game.Users.AddUser(mc.Users.GetUserByName("@brobot24"));

                    user.CurrentGame = game;

                    client.SendMessage(new GpscOut()
                    {

                    });

                    user.SendPlusWho(user, !string.IsNullOrEmpty(context.Project) && context.Project.Contains("BURNOUT5") ? "BURNOUT5" : string.Empty);

                    client.SendMessage(new PlusMgm()
                    {
                        IDENT = game.ID.ToString(),
                        NAME = game.Name,
                        ROOM = "0",
                        MAXSIZE = game.MaxSize.ToString(),
                        MINSIZE = game.MinSize.ToString(),
                        COUNT = game.Users?.Count().ToString() ?? "1",
                        PRIV = game.Priv ? "1" : "0",
                        CUSTFLAGS = game.CustFlags,
                        SYSFLAGS = game.SysFlags,
                        EVGID = "0",
                        NUMPART = "1",
                        SEED = game.Seed,
                        GPSHOST = user.Username,
                        GPSREGION = "0",
                        GAMEMODE = "0",
                        GAMEPORT = "9673",
                        VOIPPORT = "9683",
                        PARTSIZE0 = game.MaxSize.ToString(),
                        PARAMS = game.Params,
                        OPPART0 = "0",
                        OPFLAG0 = "0",
                        PRES0 = "0",
                        OPPARAM0 = "PUSMC01?????,,,-1,-1,,d",
                        OPPO1 = user.Username,
                        OPPART1 = "0",
                        OPFLAG1 = game.CustFlags,
                        PRES1 = "0",
                        OPID1 = user.ID.ToString(),
                        ADDR1 = user.Connection?.IP,
                        LADDR1 = user.Connection?.IP,
                        MADDR1 = "$000000000000",
                        OPPARAM1 = "PUSMC01?????,,,-1,-1,,d"
                    });
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
