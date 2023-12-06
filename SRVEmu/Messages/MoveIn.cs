namespace SRVEmu.Messages
{
    public class MoveIn : AbstractMessage
    {
        public override string _Name { get => "move"; }
        
        public string? NAME { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            bool remove = false;

            var mc = context as MatchmakerServer;
            if (mc == null) return;

            Model.User? user = client.User;
            if (user == null) return;

            if (user.CurrentRoom != null)
            {
                remove = true;
                NAME = user.CurrentRoom.Name;
                user.CurrentRoom.Users?.RemoveUser(user);
                user.CurrentRoom = null;
            }

            Model.Room? room = mc.Rooms.GetRoomByName(NAME);
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
