namespace MultiSocks.Aries.Messages
{
    public class Move : AbstractMessage
    {
        public override string _Name { get => "move"; }

        public string? IDENT { get; set; }
        public string? NAME { get; set; }
        public string? COUNT { get; set; }
        public string FLAGS { get; set; } = "C";
        public string? LIDENT { get; set; }
        public string? LCOUNT { get; set; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            if (context is not MatchmakerServer mc) return;

            Model.AriesUser? user = client.User;
            if (user == null) return;

            string? NAME = GetInputCacheValue("NAME");

            if (user.CurrentRoom != null)
            {
                user.CurrentRoom.Users?.RemoveUser(user);
                user.CurrentRoom = null;
            }

            Model.AriesRoom? room = mc.Rooms.GetRoomByName(NAME);
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
                this.NAME = string.Empty;
                client.SendMessage(this);
            }
        }
    }
}
