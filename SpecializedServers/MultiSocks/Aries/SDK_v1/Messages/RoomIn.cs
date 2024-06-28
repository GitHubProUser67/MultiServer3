using MultiSocks.Aries.SDK_v6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class RoomIn : AbstractMessage
    {
        public override string _Name { get => "room"; }

        public string? NAME { get; set; } // Room Name
        public string? CAT { get; set; } // Room Category
        public string? PASS { get; set; } // Password $invis$ - invisible room?
        public string? DESC { get; set; } // Description
        public string? FLAGS { get; set; } // Room Flags
        public string? CAP { get; set; } // Room Capacity
        public string? MAX { get; set; } // Room Max Capacity
        public string? IGNEXIST { get; set; } // 0 = fail if room exists,
                                              // 1 = if room exists, ignore the create failure and just move into the room
        public string? IGNPROFNAME { get; set; } // 0 = fail if room name is profane
                                                 // 1 = do not check for profane room name
        public string? LVLRNG { get; set; } // Level Range 

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            var mc = context as MatchmakerServerV1;
            if (mc == null) return;

            string? NAME = GetInputCacheValue("NAME");

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
