using MultiSocks.DirtySocks.Messages;

namespace MultiSocks.DirtySocks.Model
{
    public class Game
    {
        public int MaxSize;
        public int MinSize;
        public int ID;
        public int RoomID;
        public string CustFlags;
        public string Params;
        public string Name;
        public string Seed;
        public string SysFlags;
        public string? pass;
        public User? Host;
        public User? GPSHost;
        public bool Priv;
        public bool Started = false;

        public UserCollection Users = new();

        public Game(int maxSize, int minSize, int id, string custFlags, string @params,
                string name, bool priv, string seed, string sysFlags, string? Pass, int roomId)
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
            RoomID = roomId;
        }

        public bool RemoveUserAndCheckGameValidity(User user)
        {
            lock (Users)
            {
                Users.RemoveUser(user);

                BroadcastPopulation();

                if (Users.Count() < MinSize)
                {
                    foreach (User batchuser in Users.GetAll())
                    {
                        batchuser.CurrentGame = null;

                        batchuser.SendPlusWho(batchuser, !string.IsNullOrEmpty(batchuser.Connection?.Context.Project) && batchuser.Connection.Context.Project.Contains("BURNOUT5") ? "BURNOUT5" : string.Empty);
                    }

                    return true;
                }
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

        public void AddUser(User? user)
        {
            Users.AddUser(user);
        }

        public void KickPlayerByUsername(string? username, int reason = 0)
        {
            User? userToRemove = Users.GetAll().FirstOrDefault(user => user.Username == username);

            if (userToRemove != null)
            {
                Users.RemoveUser(userToRemove);

                if (reason == 1)
                {
                    userToRemove.CurrentGame = null;

                    userToRemove.SendPlusWho(userToRemove, !string.IsNullOrEmpty(userToRemove.Connection?.Context.Project) && userToRemove.Connection.Context.Project.Contains("BURNOUT5") ? "BURNOUT5" : string.Empty);

                    // userToRemove.Connection?.SendMessage(new PlusKik() { GAME = ID.ToString() }); // TODO, figure out why it crash client...
                }
                else if (reason == 2)
                {
                    userToRemove.CurrentGame = null;

                    userToRemove.SendPlusWho(userToRemove, !string.IsNullOrEmpty(userToRemove.Connection?.Context.Project) && userToRemove.Connection.Context.Project.Contains("BURNOUT5") ? "BURNOUT5" : string.Empty);
                }
                else
                    userToRemove.CurrentGame = null;

                BroadcastPopulation();
            }
        }

        public void SetGameStatus(bool status)
        {
            Started = status;
        }

        public void UpdatePlayerParams(User updatedUser)
        {
            User? user = Users.GetAll().FirstOrDefault(user => user.ID == updatedUser.ID);

            if (user != null)
                Users.UpdateUser(user, updatedUser);
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
                ROOM = RoomID.ToString(),
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

        public PlusGam GetPlusGam()
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

            return new PlusGam()
            {
                IDENT = ID.ToString(),
                HOST = Host?.Username,
                NAME = Name,
                ROOM = RoomID.ToString(),
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

        public PlusSesV2 GetPlusSesV2()
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

            return new PlusSesV2()
            {
                IDENT = ID.ToString(),
                HOST = Host?.Username,
                NAME = Name,
                ROOM = RoomID.ToString(),
                MAXSIZE = MaxSize.ToString(),
                MINSIZE = MinSize.ToString(),
                COUNT = Users?.Count().ToString() ?? "1",
                PRIV = Priv ? "1" : "0",
                CUSTFLAGS = CustFlags,
                SYSFLAGS = "528448",
                EVGID = "0",
                SEED = ID.ToString(),
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

        public GcreOut GetGcreOut()
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

            return new GcreOut()
            {
                IDENT = ID.ToString(),
                HOST = Host?.Username,
                NAME = Name,
                ROOM = RoomID.ToString(),
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

        public GsetOut GetGsetOut()
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

            return new GsetOut()
            {
                IDENT = ID.ToString(),
                HOST = Host?.Username,
                NAME = Name,
                ROOM = RoomID.ToString(),
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

        public GjoiOut GetGjoiOut()
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

            return new GjoiOut()
            {
                IDENT = ID.ToString(),
                HOST = Host?.Username,
                NAME = Name,
                ROOM = RoomID.ToString(),
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

        public GqwkOut GetGqwkOut()
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

            return new GqwkOut()
            {
                IDENT = ID.ToString(),
                HOST = Host?.Username,
                NAME = Name,
                ROOM = RoomID.ToString(),
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

        public GleaOut GetGleaOut()
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

            return new GleaOut()
            {
                IDENT = ID.ToString(),
                HOST = Host?.Username,
                NAME = Name,
                ROOM = RoomID.ToString(),
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

        public void BroadcastPopulation()
        {
            Users.Broadcast(GetPlusMgm());
        }

        public void BroadcastPlusSesV2()
        {
            Users.Broadcast(GetPlusSesV2());
        }
    }
}
