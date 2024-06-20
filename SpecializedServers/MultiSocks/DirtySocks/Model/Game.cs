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

        public bool RemoveUserAndCheckGameValidity(User user, int reason = 0, string? KickReason = "")
        {
            lock (Users)
            {
                Users.RemoveUser(user);

                user.CurrentGame = null;

                if (Users.Count() < MinSize || GPSHost == user) // End Game.
                {
                    if (!string.IsNullOrEmpty(user.Connection?.Context.Project))
                    {
                        if (user.Connection.Context.Project.Contains("DPR-09"))
                            user.SendPlusWho(user, "DPR-09");
                        else if (user.Connection.Context.Project.Contains("BURNOUT5"))
                            user.SendPlusWho(user, "BURNOUT5");
                        else
                            user.SendPlusWho(user, string.Empty);
                    }
                    else
                        user.SendPlusWho(user, string.Empty);

                    foreach (User batchuser in Users.GetAll())
                    {
                        Users.RemoveUser(batchuser);

                        batchuser.CurrentGame = null;

                        if (!string.IsNullOrEmpty(batchuser.Connection?.Context.Project))
                        {
                            if (batchuser.Connection.Context.Project.Contains("DPR-09"))
                                batchuser.SendPlusWho(batchuser, "DPR-09");
                            else if (batchuser.Connection.Context.Project.Contains("BURNOUT5"))
                                batchuser.SendPlusWho(batchuser, "BURNOUT5");
                            else
                                batchuser.SendPlusWho(batchuser, string.Empty);
                        }
                        else
                            batchuser.SendPlusWho(batchuser, string.Empty);
                    }

                    return true;
                }
				else
				{
					if (reason == 1)
                        user.Connection?.SendMessage(new PlusKik() { REASON = KickReason }); // Thank you Bo98!
                    else
                    {
						if (!string.IsNullOrEmpty(user.Connection?.Context.Project))
						{
							if (user.Connection.Context.Project.Contains("DPR-09"))
								user.SendPlusWho(user, "DPR-09");
							else if (user.Connection.Context.Project.Contains("BURNOUT5"))
								user.SendPlusWho(user, "BURNOUT5");
                            else
                                user.SendPlusWho(user, string.Empty);
                        }
						else
							user.SendPlusWho(user, string.Empty);
					}
					
					BroadcastPopulation();
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

        public bool RemovePlayerByUsername(string? username, int reason = 0, string? KickReason = "")
        {
            User? userToRemove = Users.GetAll().FirstOrDefault(user => user.Username == username);

            if (userToRemove != null)
                return RemoveUserAndCheckGameValidity(userToRemove, reason, KickReason);

            return false;
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
                PLAYERSLIST = GetPlayersList()
            };
        }

        public PlusGam GetPlusGam()
        {
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
                PLAYERSLIST = GetPlayersList()
            };
        }

        public PlusSesV2 GetPlusSesV2()
        {
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
                SYSFLAGS = "528448", // Needed for Burnout Paradise to start the session.
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
                PLAYERSLIST = GetPlayersList()
            };
        }

        public GcreOut GetGcreOut()
        {
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
                PLAYERSLIST = GetPlayersList()
            };
        }

        public GsetOut GetGsetOut()
        {
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
                PLAYERSLIST = GetPlayersList()
            };
        }

        public GjoiOut GetGjoiOut()
        {
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
                PLAYERSLIST = GetPlayersList()
            };
        }

        public GqwkOut GetGqwkOut()
        {
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
                PLAYERSLIST = GetPlayersList()
            };
        }

        public GleaOut GetGleaOut()
        {
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
                PLAYERSLIST = GetPlayersList()
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

        private Dictionary<string, string> GetPlayersList()
        {
            int i = 0;
            Dictionary<string, string> PLAYERSLIST = new();

            foreach (User? user in Users.GetAll())
            {
                if (user != null)
                {
                    PLAYERSLIST.Add($"OPPO{i}", user.Username);
                    PLAYERSLIST.Add($"OPPART{i}", "0");
                    PLAYERSLIST.Add($"OPFLAG{i}", "0");
                    PLAYERSLIST.Add($"PRES{i}", "0");
                    PLAYERSLIST.Add($"OPID{i}", user.ID.ToString());
                    PLAYERSLIST.Add($"ADDR{i}", user.ADDR);
                    PLAYERSLIST.Add($"LADDR{i}", user.LADDR);
                    PLAYERSLIST.Add($"MADDR{i}", user.MAC);

                    if (!string.IsNullOrEmpty(user.Connection?.Context.Project))
                    {
                        if (user.Connection.Context.Project.Contains("BURNOUT5"))
                        {
                            // Burnout uses a custom function to attribute ther player colors via the server based on player index in the game, thank you Bo98!
                            string PlayerColorModifer(int index, string param)
                            {
                                if (index == 3 && param.StartsWith("ff")) // Struct is weird as ****
                                    return (i - 1).ToString() + ',' + param[2..];

                                return param;
                            }

                            PLAYERSLIST.Add($"OPPARAM{i}", user.GetParametersString(PlayerColorModifer));
                        }
                        else
                            PLAYERSLIST.Add($"OPPARAM{i}", user.GetParametersString());
                    }
                    else
                        PLAYERSLIST.Add($"OPPARAM{i}", user.GetParametersString());
                }

                i++;
            }

            return PLAYERSLIST;
        }
    }
}
