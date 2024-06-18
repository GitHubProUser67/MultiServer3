using MultiSocks.DirtySocks.DataStore;
using MultiSocks.DirtySocks.Messages;
using MultiSocks.DirtySocks.Model;

namespace MultiSocks.DirtySocks
{
    public class EAMessengerServer : AbstractDirtySockServer
    {
        public override Dictionary<string, Type?> NameToClass { get; } =
            new Dictionary<string, Type?>()
            {
                { "AUTH", typeof(EAMAuthIn) },
                { "RGET", typeof(RgetIn) },
                { "PSET", typeof(PsetIn) },
            };

        public UserCollection Users = new();

        private readonly Thread PingThread;

        public EAMessengerServer(ushort port, bool lowlevel, List<Tuple<string, bool>>? RoomToAdd = null, string? Project = null, string? SKU = null, bool secure = false, string CN = "", string email = "", bool WeakChainSignedRSAKey = false) : base(port, lowlevel, Project, SKU, secure, CN, email, WeakChainSignedRSAKey)
        {
            lock (Users)
                Users.AddUser(new User() { Username = "@brobot24", ID = 24 }); // Admin player.

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

        public override void RemoveClient(DirtySockClient client)
        {
            base.RemoveClient(client);

            //clean up this user's state.
            //are they logged in?
            User? user = client.User;
            if (user != null)
            {
                Users.RemoveUser(user);

            }
        }

        public void TryEAMLogin(DbAccount user, DirtySockClient client, string VERS = "")
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
            };

            Users.AddUser(user2);
            client.User = user2;

            // Ideally all the infos comes from User profile and not hardcodded.


            client.SendMessage(new EAMAuthOut()
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
        }
    }
}
