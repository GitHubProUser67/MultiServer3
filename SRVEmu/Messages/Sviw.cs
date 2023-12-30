using SRVEmu.Model;

namespace SRVEmu.Messages
{
    public class Sviw : AbstractMessage
    {
        public override string _Name { get => "sviw"; }

        public string? PERS { get; set; }
        public string? VIEW { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as MatchmakerServer;
            if (mc == null) return;

            User? user = client.User;
            if (user == null) return;

            if (VIEW == "DLC")
            {
                string? RoomName = "OnlineFreeBurnLobby";
                if (user.CurrentRoom != null)
                    RoomName = user.CurrentRoom.Name;
                Room? room = mc.Rooms.GetRoomByName(RoomName);
                if (room != null)
                {
                    if (room.Users != null)
                        room.Users.SendPlusWho(user);
                }

                client.SendMessage(new Dlc());
            }
            else
            {
                if (user.SelectedPersona != -1) return;
                user.SelectPersona(PERS);
                if (user.SelectedPersona == -1) return; //failed?
                client.SendMessage(new PersOut()
                {
                    NAME = user.Username,
                    PERS = user.PersonaName
                });
            }

            client.SendMessage(new Ping()
            {

            });
        }
    }
}
