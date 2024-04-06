namespace MultiSocks.DirtySocks.Messages
{
    public class MoveIn : AbstractMessage
    {
        public override string _Name { get => "move"; }

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
                if (room.Users != null && !room.Users.AddUser(user))
                {
                    client.SendMessage(new MoveFull());
                    return;
                }
                user.CurrentRoom = room;
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

    public class MoveFull : AbstractMessage
    {
        public override string _Name { get => "movefull"; }
    }
}
