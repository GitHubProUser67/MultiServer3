using MultiSocks.Aries.SDK_v6.Messages;
using MultiSocks.Aries.SDK_v6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class PeekIn : AbstractMessage
    {
        public override string _Name { get => "peek"; }

        public string? NAME { get; set; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            MatchmakerServerV1? mc = context as MatchmakerServerV1;
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
