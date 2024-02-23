using SRVEmu.DirtySocks.Model;

namespace SRVEmu.DirtySocks.Messages
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
        public string USERFLAGS { get; set; } = "0";

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as MatchmakerServer;
            if (mc == null) return;

            bool remove = false;

            User? user = client.User;
            if (user == null) return;

            string? RoomName = NAME;

            if (user.CurrentRoom != null)
            {
                remove = true;
                RoomName = user.CurrentRoom.Name;
                user.CurrentRoom.Users?.RemoveUser(user);
                user.CurrentRoom = null;
            }
            Room? room = mc.Rooms.GetRoomByName(RoomName);
            if (room != null)
            {
                if (room.Users != null)
                {
                    if (remove)
                    {
                        room.Users.RemoveUser(user);
                        client.SendMessage(new MoveOut()
                        {
                            NAME = string.Empty
                        });
                    }
                    else
                    {
                        if (!room.Users.AddUser(user))
                        {
                            client.SendMessage(new MoveFull());
                            return;
                        }
                        user.CurrentRoom = room;
                    }
                }
                else
                {
                    client.SendMessage(new MoveOut()
                    {
                        NAME = string.Empty
                    });
                }
            }
            else
            {
                mc.Rooms.AddRoom(new Room() { Name = RoomName, IsGlobal = true });
                room = mc.Rooms.GetRoomByName(RoomName);
                if (room != null)
                {
                    if (room.Users != null)
                    {
                        if (remove)
                        {
                            room.Users.RemoveUser(user);
                            client.SendMessage(new MoveOut()
                            {
                                NAME = string.Empty
                            });
                        }
                        else
                        {
                            if (!room.Users.AddUser(user))
                            {
                                client.SendMessage(new MoveFull());
                                return;
                            }
                            user.CurrentRoom = room;
                        }
                    }
                    else
                    {
                        client.SendMessage(new MoveOut()
                        {
                            NAME = string.Empty
                        });
                    }
                }
            }

            client.SendMessage(new PlusAgm()
            {
                IDENT = user.ID.ToString(),
                NAME = NAME,
                HOST = user.Username,
                PARAMS = PARAMS,
                ROOM = user.CurrentRoom?.Name,
                MAXSIZE = MAXSIZE,
                MINSIZE = MINSIZE,
                USERFLAGS = USERFLAGS,
                SYSFLAGS = SYSFLAGS,
                PASS = PASS,
                PRIV = PRIV,
                SEED = SEED,
                CUSTFLAGS = CUSTFLAGS,
            });

            /*client.SendMessage(new PlusMgm()
            {
                ADDR1 = client.IP,
                CUSTFLAGS = CUSTFLAGS,
                GPSHOST = NAME,
                NAME = NAME,
                OPPO1 = NAME,
                PARAMS = PARAMS,
                PRIV = PRIV,
                SEED = SEED,
                SYSFLAGS = SYSFLAGS,
                OPID1 = user.ID.ToString(),
                OPFLAG1 = USERFLAGS
            });*/

            client.SendMessage(new GpscOut() // Game will disconnect if this command is not sent, perhaps a little clue.
            {
                GPSHOST = user.Username,
                HOST = user.Username
            });
        }
    }
}
