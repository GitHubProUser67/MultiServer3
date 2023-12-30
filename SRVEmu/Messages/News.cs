using BackendProject;
using SRVEmu.Model;
using System.Text;

namespace SRVEmu.Messages
{
    public class News : AbstractMessage
    {
        public override string _Name { get => "news"; }

        public string? NAME { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            MatchmakerServer? mc = context as MatchmakerServer;
            if (mc == null) return;

            if (NAME == "client.cfg") // TODO, do a real config file.
            {
                byte[] newstext = Encoding.ASCII.GetBytes($"BUDDY_SERVER={MiscUtils.GetLocalIPAddress()}\nBUDDY_PORT=10899 BUDDY_URL=http://ps3burnout08.ea.com/\n" +
                    "TOSAC_URL=http://ps3burnout08.ea.com/TOSAC.txt\nTOSA_URL=http://ps3burnout08.ea.com/TOSA.txt\n" +
                    "TOS_URL=http://ps3burnout08.ea.com/TOS.txt\nNEWS_TEXT=VTSTech.is.reviving.games_NEWS\n" +
                    "TOS_TEXT=VTSTech.is.reviving.games_TOS\nNEWS_DATE=2023.12.06-02:48:57 NEWS_URL=http://ps3burnout08.ea.com/news.txt\n"); // Lenght is VERY IMPORTANT.

                client.SendMessage(MiscUtils.CombineByteArrays(new byte[] { 0x6e, 0x65, 0x77, 0x73, 0x6e, 0x65, 0x77, 0x37, 0x00, 0x00, 0x01, 0x80 }, new byte[][]
                                   {
                                       newstext,
                                       new byte[] { 0x00 }
                                   }));
            }
            else if (NAME == "8") // TODO, should we really create room here?
            {
                bool remove = false;

                User? user = client.User;
                if (user == null) return;

                string? RoomName = "OnlineFreeBurnLobby";

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

                byte[] newstext = Encoding.ASCII.GetBytes($"BUDDY_SERVER={MiscUtils.GetLocalIPAddress()}\nBUDDY_PORT=10899 BUDDY_URL=http://ps3burnout08.ea.com/\n" +
                    "TOSAC_URL=http://ps3burnout08.ea.com/TOSAC.txt\nTOSA_URL=http://ps3burnout08.ea.com/TOSA.txt\n" +
                    "TOS_URL=http://ps3burnout08.ea.com/TOS.txt\nNEWS_TEXT=VTSTech.is.reviving.games_NEWS\n" +
                    "TOS_TEXT=VTSTech.is.reviving.games_TOS\nNEWS_DATE=2023.12.06-02:48:57 NEWS_URL=http://ps3burnout08.ea.com/news.txt\n"); // Lenght is VERY IMPORTANT.

                client.SendMessage(MiscUtils.CombineByteArrays(new byte[] { 0x6e, 0x65, 0x77, 0x73, 0x6e, 0x65, 0x77, 0x38, 0x00, 0x00, 0x01, 0x80 }, new byte[][]
                                   {
                                       newstext,
                                       new byte[] { 0x00 }
                                   }));
            }
            else
                CustomLogger.LoggerAccessor.LogWarn($"[DirtySocks] - News - Client Requested an unknown config type: {NAME}, not responding");
        }
    }
}
