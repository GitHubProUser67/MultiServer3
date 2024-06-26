using MultiSocks.DirtySocks.DataStore;
using MultiSocks.DirtySocks.Messages;
using MultiSocks.DirtySocks.Model;

namespace MultiSocks.DirtySocks
{
    public class MatchmakerServer : AbstractDirtySockServer
    {
        public override Dictionary<string, Type?> NameToClass { get; } =
            new Dictionary<string, Type?>()
            {
                { "~png", typeof(Ping) },
                { "move", typeof(MoveIn) }, //move into a room. remove last room and broadcast "+rom" to others. broadcast "+pop" with Z=ID/Count for population update
                { "mesg", typeof(Mesg) }, //PRIV is non-null for private. else broadcast to room. PRIV->(find client), TEXT->T, ATTR->EP, (name)->N
                { "room", typeof(RoomIn) }, //create room. NAME=name, return (room, +who, +msg, +rom to all, +usr)
                { "auxi", typeof(Auxi) }, //auxiliary information. returned as X attribute in +usr and +who
                { "auth", typeof(AuthIn) },
                { "acct", typeof(AcctIn) },
                { "cate", typeof(CateIn) }, //?
                { "conn", null }, //?
                { "cusr", null }, //?
                { "cper", typeof(CperIn) }, //create persona. (NAME) in, (PERS, NAME) out. where name is username.
                { "dper", typeof(DperIn) }, //delete persona
                { "edit", null }, //?
                { "fbst", typeof(FbstIn) }, //?
                { "fget", typeof(FgetIn) }, //?
                { "fupd", typeof(FupdIn) }, // Update friend/rival lists in user record
                { "fupr", typeof(FuprIn) }, //?
                { "hchk", typeof(HchkIn) }, //?
                { "peek", typeof(PeekIn) }, //Audit room and receive infos about it, but not enter.
                { "pent", typeof(PentIn) }, // Purchase entitlement.
                { "pers", typeof(PersIn) }, //select persona
                { "sdta", typeof(SdtaIn) }, //?
                { "sele", typeof(SeleIn) }, //gets info for the current server
                { "skey", typeof(SKeyIn) }, //session key?
                { "slst", typeof(SlstIn) }, //lobby details?
                { "sviw", typeof(Sviw) }, // Request names for each fields in a user's stats record.
                { "user", typeof(UserIn) }, //get my user info
                { "usld", typeof(UsldIn) }, // User Settings Load -- load a user's (persona's) common game specific settings.
                { "onln", typeof(OnlnIn) }, //search for a user's info
                { "opup", typeof(OpupIn) }, //?
                { "rent", typeof(RentIn) }, // Refresh Entitlements
                { "rrlc", typeof(RrlcIn) }, // (CUSTOM Burnout Paradise) Road Rules Local
                { "rrup", typeof(RrupIn) }, // (CUSTOM Burnout Paradise) Road Rules Upload
                { "rrgt", typeof(RrgtIn) }, // (CUSTOM Burnout Paradise) Road Rules Get
                { "rvup", typeof(RvupIn) }, // (CUSTOM Burnout Paradise) RiVal UPload
                { "addr", typeof(Addr) }, //the client tells us their IP and port (ephemeral). The IP is usually wrong.
                { "chal", typeof(Chal) }, //enter challenge mode
                { "glea", typeof(GleaIn) }, //leave game (string NAME)
                { "gget", null }, //get game by name
                { "gjoi", typeof(GjoiIn) }, //join game by name
                { "gdel", typeof(GdelIn) }, //delete game by name
                { "gsea", typeof(GseaIn) }, //Game search
                { "gset", typeof(GsetIn) }, //Actualize Game properties.
                { "gsta", typeof(GstaIn) }, //game start. return gstanepl if not enough people, empty gsta if enough.
                { "gcre", typeof(GcreIn) }, //game create. (name, roomname, maxPlayers, minPlayers, sysFlags, params). return a lot of info
                { "gpsc", typeof(GpscIn) }, // Create a game on a persistent game spawn service for a user.
                { "gqwk", typeof(GqwkIn) }, // Join the best matching game based on provided criteria.
                { "news", typeof(NewsIn) }, //news for server. return newsnew0 with news info (plaintext mode, NOT keyvalue)
                { "rank", typeof(RankIn) }, //unknown. { RANK = "Unranked", TIME = 866 }
                { "tcup", typeof(TcupIn) }, // Time Challenge Score Upload?
                { "snap", typeof(SnapIn) }, // Get Leaderboard snapshot
                { "quik", typeof(QuikIn) }, // Old version Quick Match
                { "rept", typeof(ReptIn) }, // Submit a Report about a user
                { "rcat", typeof(RcatIn) }, // Fetch room category information
                { "priv", typeof(PrivIn) }, // Set Private Message mode.
                { "flag", typeof(FlagIn) }, // Set attribute flags.
                { "ucre", typeof(UcreIn) }, // Create a new user set. A user set maps a set of online users into a group.
                { "uatr", typeof(UatrIn) }, // Update user attributes and hardware flags
                { "lggr", typeof(Lggr) } // Client -> Server logger 
            };

        public UserCollection Users = new();
        public RoomCollection Rooms = new();
        public GameCollection Games = new();

        private readonly Thread PingThread;

        public MatchmakerServer(ushort port, bool lowlevel, List<Tuple<string, bool>>? RoomToAdd = null, string? Project = null, string? SKU = null, bool secure = false, string CN = "", string email = "", bool WeakChainSignedRSAKey = false) : base(port, lowlevel, Project, SKU, secure, CN, email, WeakChainSignedRSAKey)
        {
            Rooms.Server = this;

            lock (Users)
                Users.AddUser(new User() { ID = 24 }); // Admin player.

            PingThread = new Thread(PingLoop);
            PingThread.Start();

            lock (Rooms)
            {
                if (RoomToAdd != null)
                {
                    foreach (var pair in RoomToAdd)
                    {
                        CustomLogger.LoggerAccessor.LogInfo($"[MatchmakerServer] - Adding Room: {pair.Item1}, {(pair.Item2 ? ("With Global Availability: " + pair.Item2 + " ") : string.Empty)}on Port: {port}");
                        Rooms.AddRoom(new Room() { Name = pair.Item1, IsGlobal = pair.Item2 });
                    }
                }
            }
        }

        public void PingLoop()
        {
            while (true)
            {
                Broadcast(new Ping());
                Thread.Sleep(30000);
            }
        }

        public override void RemoveClient(DirtySockClient client)
        {
            base.RemoveClient(client);

            //clean up this user's state.
            //are they logged in?
            User? user = client.User;
            if (user != null)
            {
                Users.RemoveUser(user);

                Game? game = user.CurrentGame;
                Room? room = user.CurrentRoom;
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

                if (room != null)
                {
                    room.Users?.RemoveUser(user);
                    user.CurrentRoom = null;
                }
            }
        }

        public void SendToPersona(string name, AbstractMessage msg)
        {
            User? user = Users.GetUserByPersonaName(name);
            if (user != null)
                user.Connection?.SendMessage(msg);
        }

        public void TryLogin(DbAccount user, DirtySockClient client, string MAC = "", string VERS = "")
        {
            //is someone else already logged in as this user?
            User? oldUser = Users.GetUserByName(user.Username);
            if (oldUser != null)
            {
                oldUser.Connection?.Close(); //should remove the old user.
                Thread.Sleep(500);
            }

            string[] personas = new string[4];
            for (int i = 0; i < user.Personas.Count; i++)
            {
                personas[i] = user.Personas[i];
            }

            // make a user object from DB user
            User user2 = new()
            {
                MAC = MAC,
                Connection = client,
                ID = user.ID,
                Personas = personas,
                Username = user.Username,
                ADDR = client.ADDR,
                LADDR = client.LADDR
            };

            Users.AddUser(user2);
            client.User = user2;

            // Ideally all the infos comes from User profile and not hardcodded.

            if (VERS.Contains("BURNOUT5"))
                client.SendMessage(new AuthOut()
                {
                    LAST = "2018.1.1-00:00:00",
                    TOS = user.TOS,
                    SHARE = user.SHARE,
                    LUID = "$00000000000003fe",
                    NAME = user.Username,
                    PERSONAS = string.Join(',', user.Personas),
                    MAIL = user.MAIL,
                    BORN = "19700101",
                    FROM = "FR",
                    LOC = "frFR",
                    SPAM = "YN",
                    SINCE = "2008.1.1-00:00:00",
                    GFIDS = "1",
                    ADDR = client.ADDR,
                    TOKEN = "pc6r0gHSgZXe1dgwo_CegjBCn24uzUC7KVq1LJDKJ0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"
                });
            else
                client.SendMessage(new AuthOut()
                {
                    BORN = "19800325",
                    GEND = "M",
                    FROM = "US",
                    LANG = "en",
                    LAST = "2003.12.8 15:51:38",
                    TOS = user.TOS,
                    NAME = user.Username,
                    MAIL = user.MAIL,
                    PERSONAS = string.Join(',', user.Personas)
                });

            Rooms.SendRooms(user2);
        }
    }
}
