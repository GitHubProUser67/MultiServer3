using MultiSocks.DirtySocks.Messages;

namespace MultiSocks.DirtySocks.Model
{
    public class Game
    {
        public int MaxSize;
        public int MinSize;
        public int ID;

        public string CustFlags;
        public string Params;
        public string Name;
        public string Seed;
        public string SysFlags;
        public string? pass;
        public User? Host;
        public User? GPSHost;

        public bool Priv;

        public UserCollection Users = new();

        public Game(int maxSize, int minSize, int id, string custFlags, string @params,
                string name, bool priv, string seed, string sysFlags, string? Pass)
        {
            MaxSize = maxSize;
            MinSize = minSize;
            ID = id;
            CustFlags = custFlags;
            Params = @params;
            Name = name;
            Priv = priv;
            Seed = seed;
            SysFlags = sysFlags;
            pass = Pass;
        }

        public bool RemoveUserAndCheckGameValidity(User user)
        {
            lock (Users)
            {
                Users.RemoveUser(user);

                if (Users.Count() < MinSize)
                    return true;
            }

            return false;
        }

        public void AddHost(User? user)
        {
            Users.AddUser(user);
            Host = user;
        }

        public void AddGPSHost(User? user)
        {
            Users.AddUser(user);
            GPSHost = user;
            Host ??= user;
        }

        public PlusMgm GetPlusMgm()
        {
            int i = 0;
            Dictionary<string, string> PLAYERSLIST = new();

            foreach (User? user in Users.GetAll())
            {
                if (user != null)
                {
                    PLAYERSLIST.Add($"OPPO{i}", user.Username ?? "@brobot24");
                    PLAYERSLIST.Add($"OPPART{i}", "0");
                    PLAYERSLIST.Add($"OPFLAG{i}", "0");
                    PLAYERSLIST.Add($"PRES{i}", "0");
                    PLAYERSLIST.Add($"OPID{i}", user.ID.ToString());
                    PLAYERSLIST.Add($"ADDR{i}", ((user.Username ?? "@brobot24") == "@brobot24") ? "127.0.0.1" : user.Connection?.IP ?? "127.0.0.1");
                    PLAYERSLIST.Add($"LADDR{i}", ((user.Username ?? "@brobot24") == "@brobot24") ? "127.0.0.1" : user.Connection?.IP ?? "127.0.0.1");
                    PLAYERSLIST.Add($"MADDR{i}", string.Empty);
                    PLAYERSLIST.Add($"OPPARAM{i}", user.Params);
                }

                i++;
            }

            return new PlusMgm()
            {
                IDENT = ID.ToString(),
                HOST = Host?.Username,
                NAME = Name,
                ROOM = "0",
                MAXSIZE = MaxSize.ToString(),
                MINSIZE = MinSize.ToString(),
                COUNT = Users?.Count().ToString() ?? "1",
                PRIV = Priv ? "1" : "0",
                CUSTFLAGS = CustFlags,
                SYSFLAGS = SysFlags,
                EVGID = "0",
                SEED = Seed,
                GPSHOST = GPSHost?.Username,
                GPSREGION = "0",
                GAMEMODE = "0",
                GAMEPORT = "9673",
                VOIPPORT = "9683",
                PARAMS = Params,
                NUMPART = "1",
                PARTSIZE0 = MaxSize.ToString(),
                PARTPARAMS0 = string.Empty,
                PLAYERSLIST = PLAYERSLIST
            };
        }
    }
}
