using MultiSocks.Aries.DataStore;
using MultiSocks.Aries.SDK_v6.Messages;
using MultiSocks.Aries.SDK_v6.Messages.BurnoutParadisePlugin;
using MultiSocks.Aries.SDK_v6.Messages.ErrorCodes;
using MultiSocks.Aries.SDK_v6.Model;
using MultiSocks.Utils;

namespace MultiSocks.Aries.SDK_v6
{
    public class MatchmakerServerV6 : AbstractAriesServerV6
    {
        public override Dictionary<string, Type?> NameToClass { get; } =
            new Dictionary<string, Type?>()
            {
                { "~png", typeof(Ping) },
                { "auth", typeof(Auth) },
                { "acct", typeof(Acct) },
                { "cate", typeof(Cate) }, //?
                { "fbst", typeof(Fbst) }, //?
                { "fget", typeof(Fget) }, //?
                { "fupd", typeof(Fupd) }, // Update friend/rival lists in user record
                { "fupr", typeof(Fupr) }, //?
                { "hchk", typeof(Hchk) }, //?
                { "pent", typeof(Pent) }, // Purchase entitlement.
                { "pers", typeof(PersIn) }, //select persona
                { "sdta", typeof(Sdta) }, //?
                { "sele", typeof(SeleIn) }, //gets info for the current server
                { "skey", typeof(SKey) }, //session key?
                { "slst", typeof(Slst) }, //lobby details?
                { "snap", typeof(Snap) }, //leaderboard snapshot
                { "sviw", typeof(Sviw) }, //pers with Ping.
                { "usld", typeof(Usld) }, // User Settings Load -- load a user's (persona's) common game specific settings.
                { "onln", typeof(OnlnIn) }, //search for a user's info
                { "opup", typeof(Opup) }, //?
                { "rent", typeof(Rent) }, // Refresh Entitlements
                { "rrlc", typeof(Rrlc) }, // (CUSTOM Burnout Paradise) Road Rules Local
                { "rrup", typeof(Rrup) }, // (CUSTOM Burnout Paradise) Road Rules Upload
                { "rrgt", typeof(Rrgt) }, // (CUSTOM Burnout Paradise) Road Rules Get
                { "rvup", typeof(Rvup) }, // (CUSTOM Burnout Paradise) RiVal UPload
                { "addr", typeof(Addr) }, //the client tells us their IP and port (ephemeral). The IP is usually wrong.
                { "glea", typeof(Glea) }, //leave game (string NAME)
                { "gjoi", typeof(Gjoi) }, //join game by name
                { "gdel", typeof(Gdel) }, //delete game by name
                { "gsea", typeof(Gsea) }, //Game search
                { "gset", typeof(Gset) }, //Actualize Game properties.
                { "gsta", typeof(Gsta) }, //game start. return gstanepl if not enough people, empty gsta if enough.
                { "gcre", typeof(Gcre) }, //game create. (name, roomname, maxPlayers, minPlayers, sysFlags, params). return a lot of info
                { "gpsc", typeof(Gpsc) }, // Create a game on a persistent game spawn service for a user.
                { "gqwk", typeof(Gqwk) }, // Join the best matching game based on provided criteria.
                { "news", typeof(NewsIn) }, //news for server. return newsnew0 with news info (plaintext mode, NOT keyvalue)
                { "rank", typeof(Rank) }, //unknown. { RANK = "Unranked", TIME = 866 }
                { "tcup", typeof(Tcup) }, // Time Challenge Score Upload?
            };

        public UserCollection Users = new();
        public GameCollection Games = new();

        private readonly Thread PingThread;

        public MatchmakerServerV6(ushort port, string listenIP, string? Project = null, string? SKU = null, bool secure = false, string CN = "", string email = "", bool WeakChainSignedRSAKey = false) : base(port, listenIP, Project, SKU, secure, CN, email, WeakChainSignedRSAKey)
        {
            lock (Users)
                Users.AddUser(new User() { ID = 24 }); // Admin player.

            PingThread = new Thread(PingLoop);
            PingThread.Start();
        }

        public void PingLoop()
        {
            while (true)
            {
                Broadcast(new Ping());
                Thread.Sleep(30000);
            }
        }

        public Task BroadcastGamesListDetails()
        {
            foreach (Game game in Games.GamesSessions.Values)
            {
                Broadcast(game.GetGameDetails("+gam"));
            }

            return Task.CompletedTask;
        }

        public override void RemoveClient(AriesClient client)
        {
            base.RemoveClient(client);

            //clean up this user's state.
            //are they logged in?
            User? user = client.User;
            if (user != null)
            {
                Users.RemoveUser(user);

                Game? game = user.CurrentGame;
                if (game != null)
                {
                    if (game.RemoveUserAndCheckGameValidity(user))
                    {
                        lock (Games)
                        {
                            Games.RemoveGame(game);
                        }
                    }
                }
            }
        }

        public void SendToPersona(string name, AbstractMessage msg)
        {
            User? user = Users.GetUserByPersonaName(name);
            user?.Connection?.SendMessage(msg);
        }

        public void TryLogin(DbAccount user, AriesClient client, string? PASS, string LOC, string? MAC)
        {
            //is someone else already logged in as this user?
            User? oldUser = Users.GetUserByName(user.Username);
            if (oldUser != null)
            {
                client.SendMessage(new AuthLogn());
                return;
            }

            PasswordUtils passutils = new();

            string? DecryptedPass = passutils.ssc2Decode(PASS, client.SKEY);

            if (DecryptedPass != null && DecryptedPass == string.Empty && ("PS3".Equals(client.SKU))) // EA assumed that Consoles protect the login so they crypt an empty password, extremly bad, but can't do anything.
            {
                
            }
            else
            {
                if (!string.IsNullOrEmpty(user.Password))
                {
                    if (!user.Password.Equals(DecryptedPass))
                    {
                        client.SendMessage(new AuthPass());
                        return;
                    }
                }
            }

            CustomLogger.LoggerAccessor.LogInfo("Logged in: " + user.Username);

            string[] personas = new string[user.Personas.Count];
            for (int i = 0; i < user.Personas.Count; i++)
            {
                personas[i] = user.Personas[i];
            }

            // make a user object from DB user
            User user2 = new()
            {
                MAC = MAC ?? string.Empty,
                Connection = client,
                ID = user.ID,
                Personas = personas,
                Username = user.Username,
                ADDR = client.ADDR,
                LADDR = client.LADDR,
                LOC = LOC
            };

            Users.AddUser(user2);

            client.User = user2;

            Dictionary<string, string?> OutputCache = new() {
                { "LAST", "2018.1.1-00:00:00" },
                { "TOS", user.TOS },
                { "SHARE", user.SHARE },
                { "_LUID", "$00000000000003fe" },
                { "NAME", user.Username },
                { "PERSONAS", string.Join(',', user.Personas) },
                { "MAIL", user.MAIL }, { "BORN", "19700101" },
                { "FROM", user2.LOC[2..] },
                { "LOC", user2.LOC },
                { "SPAM", "NN" },
                { "SINCE", "2008.1.1-00:00:00" },
                { "GFIDS", "1" },
                { "ADDR", client.ADDR },
                { "TOKEN", "pc6r0gHSgZXe1dgwo_CegjBCn24uzUC7KVq1LJDKJ0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" }};

            client.SendMessage(new Auth() { OutputCache = OutputCache });
        }
    }
}
