namespace MultiSocks.DirtySocks.Messages
{
    public class PeekIn : AbstractMessage
    {
        public override string _Name { get => "peek"; }

        public string? NAME { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            MatchmakerServer? mc = context as MatchmakerServer;
            if (mc == null) return;

            Model.User? user = client.User;
            if (user == null) return;

            if (string.IsNullOrEmpty(NAME))
            {
                client.SendMessage(new PeekOut()
                {
                    NAME = string.Empty
                });
                return;
            }

            Model.Room? room = mc.Rooms.GetRoomByName(NAME);
            if (room != null)
                room.Users?.AuditRoom(user);
            else
                client.SendMessage(new PeekOut()
                {
                    NAME = string.Empty
                });
        }
    }

    public class UnPeek : AbstractMessage
    {
        public override string _Name { get => "unpeek"; }
    }
}
