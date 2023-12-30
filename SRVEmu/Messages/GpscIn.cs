using SRVEmu.Model;

namespace SRVEmu.Messages
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
        public string? USERFLAGS { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as MatchmakerServer;
            if (mc == null) return;

            User? user = client.User;
            if (user == null) return;


            client.SendMessage(new GpscOut()
            {
                CUSTFLAGS = CUSTFLAGS,
                MINSIZE = MINSIZE,
                MAXSIZE = MAXSIZE,
                NAME = NAME,
                PARAMS = PARAMS,
                PASS = PASS,
                PRIV = PRIV,
                SEED = SEED,
                SYSFLAGS = SYSFLAGS,
                FORCE_LEAVE = FORCE_LEAVE,
                USERPARAMS = USERPARAMS,
                USERFLAGS = USERFLAGS,
            });

            string? RoomName = "OnlineFreeBurnLobby";
            if (user.CurrentRoom != null)
                RoomName = user.CurrentRoom.Name;
            Room? room = mc.Rooms.GetRoomByName(RoomName);
            if (room != null)
            {
                if (room.Users != null)
                    room.Users.SendPlusWho(user);
            }

            client.SendMessage(new PlusMgm()
            {

            });
        }
    }
}
