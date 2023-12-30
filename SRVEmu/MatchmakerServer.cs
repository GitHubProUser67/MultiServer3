using SRVEmu.DataStore;
using SRVEmu.Messages;
using SRVEmu.Model;
using System;

namespace SRVEmu
{
    public class MatchmakerServer : AbstractDirtySockServer
    {
        public override Dictionary<string, Type?> NameToClass { get; } =
            new Dictionary<string, Type?>()
            {
                { "~png", typeof(Ping) },
                { "move", typeof(MoveIn) }, //move into a room. remove last room and broadcast "+rom" to others. broadcast "+pop" with Z=ID/Count for population update
                { "mesg", typeof(Mesg) }, //PRIV is non-null for private. else broadcast to room. PRIV->(find client), TEXT->T, ATTR->EP, (name)->N
                { "room", null }, //create room. NAME=name, return (room, +who, +msg, +rom to all, +usr)
                { "auxi", typeof(Auxi) }, //auxiliary information. returned as X attribute in +usr and +who
                { "auth", typeof(AuthIn) },
                { "acct", typeof(AcctIn) },
                { "cate", null }, //?
                { "conn", null }, //?
                { "cusr", null }, //?
                { "cper", typeof(CperIn) }, //create persona. (NAME) in, (PERS, NAME) out. where name is username.
                { "dper", typeof(DperIn) }, //delete persona
                { "edit", null }, //?
                { "fget", null }, //?
                { "fupd", null }, //Room equiv
                { "peek", null }, //?
                { "pers", typeof(PersIn) }, //select persona
                { "sdta", typeof(SdtaIn) }, //?
                { "sele", typeof(SeleIn) }, //gets info for the current server
                { "skey", typeof(SKeyIn) }, //session key?
                { "slst", typeof(SlstIn) }, //lobby details?
                { "sviw", typeof(Sviw) }, //pers with Ping.
                { "user", typeof(UserIn) }, //get my user info
                { "usld", typeof(UsldIn) }, //Ping Equiv?
                { "onln", typeof(OnlnIn) }, //search for a user's info
                { "rvup", null }, //?
                { "addr", typeof(Addr) }, //the client tells us their IP and port (ephemeral). The IP is usually wrong.
                { "chal", typeof(Chal) }, //enter challenge mode
                { "glea", null }, //leave game (string NAME)
                { "gget", null }, //get game by name
                { "gjoi", null }, //join game by name
                { "gdel", null }, //delete game by name
                { "gsea", null }, //?
                { "gsta", null }, //game start. return gstanepl if not enough people, empty gsta if enough.
                { "gcre", null }, //game create. (name, roomname, maxPlayers, minPlayers, sysFlags, params). return a lot of info
                { "gpsc", typeof(GpscIn) }, //?
                { "gqwk", typeof(GqwkIn) }, //Quick join.
                { "news", typeof(News) }, //news for server. return newsnew0 with news info (plaintext mode, NOT keyvalue)
                { "rank", null }, //unknown. { RANK = "Unranked", TIME = 866 }
            };

        public UserCollection Users = new();
        public RoomCollection Rooms = new();

        public IDatabase Database;
        private Thread PingThread;

        public MatchmakerServer(ushort port, bool lowlevel, List<Tuple<string, bool>>? RoomToAdd = null) : base(port, lowlevel)
        {
            Database = new JSONDatabase();
            Rooms.Server = this;

            PingThread = new Thread(PingLoop);
            PingThread.Start();

            if (RoomToAdd != null)
            {
                foreach (var pair in RoomToAdd)
                {
                    CustomLogger.LoggerAccessor.LogInfo($"[MatchmakerServer] - Adding Room: {pair.Item1}, With Global Availability: {pair.Item2} on Port: {port}");
                    Rooms.AddRoom(new Room() { Name = pair.Item1, IsGlobal = pair.Item2 });
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
                    game.Users.RemoveUser(user);
                    user.CurrentGame = null;
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

        public void TryLogin(DbAccount user, DirtySockClient client, string VERS = "")
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

            //make a user object from DB user
            User user2 = new()
            {
                Connection = client,
                ID = user.ID,
                Personas = personas,
                Username = user.Username,
                IP = client.IP
            };

            Users.AddUser(user2);
            client.User = user2;

            client.SendMessage(new AuthOut()
            {
                TOS = user.TOS,
                NAME = user.Username,
                MAIL = user.MAIL,
                PERSONAS = string.Join(',', user.Personas)
            });

            Rooms.SendRooms(user2);
        }
    }
}
