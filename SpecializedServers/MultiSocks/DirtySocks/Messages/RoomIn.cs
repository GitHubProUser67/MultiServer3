namespace MultiSocks.DirtySocks.Messages
{
    public class RoomIn : AbstractMessage
    {
        public override string _Name { get => "room"; }

        public string? NAME { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as MatchmakerServer;
            if (mc == null) return;

            Model.User? user = client.User;
            if (user == null) return;

            if (user.CurrentRoom != null)
            {
                user.CurrentRoom.Users?.RemoveUser(user);
                user.CurrentRoom = null;
            }

            Model.Room? room = mc.Rooms.GetRoomByName(NAME);
            if (room != null)
            {
                if (room.Users != null && room.Users.AddUserWithRoomMesg(user))
                    user.CurrentRoom = room;
                else
                    client.SendMessage(new RoomOut { NAME = string.Empty });
            }
            else
            {
                room = new Model.Room() { ID = new Random().Next(), Name = NAME };
                mc.Rooms.AddRoom(room);

                if (room.Users != null && room.Users.AddUserWithRoomMesg(user))
                    user.CurrentRoom = room;
                else
                    client.SendMessage(new RoomOut { NAME = string.Empty });
            }
        }
    }
}
