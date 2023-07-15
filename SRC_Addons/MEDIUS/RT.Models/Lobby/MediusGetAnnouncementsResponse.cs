using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Common.Stream;

namespace PSMultiServer.SRC_Addons.MEDIUS.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobby, MediusLobbyMessageIds.GetAnnouncementsResponse)]
    public class MediusGetAnnouncementsResponse : BaseLobbyMessage, IMediusResponse
    {
        public override byte PacketType => (byte)MediusLobbyMessageIds.GetAnnouncementsResponse;

        public bool IsSuccess => StatusCode >= 0;

        public MessageId MessageID { get; set; }

        public MediusCallbackStatus StatusCode;
        public int AnnouncementID;
        public string Announcement; // ANNOUNCEMENT_MAXLEN
        public bool EndOfList;

        public override void Deserialize(MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            //
            MessageID = reader.Read<MessageId>();

            //
            reader.ReadBytes(3);
            StatusCode = reader.Read<MediusCallbackStatus>();
            AnnouncementID = reader.ReadInt32();

                
            // && reader.AppId == 24000 && reader.AppId == 24180 && reader.AppId == 20095 && reader.AppId != 22920 && reader.AppId != 21743 && reader.AppId == 10984
            if (reader.MediusVersion <= 112)
            {
                Announcement = reader.ReadString(Constants.ANNOUNCEMENT_MAXLEN);
            } else if (reader.MediusVersion == 113 /*&& reader.AppId == 21731 && reader.AppId == 21741 && reader.AppId == 21834 && reader.AppId == 20174*/) {
                Announcement = reader.ReadString(Constants.ANNOUNCEMENT1_MAXLEN);
            } else {
                Announcement = reader.ReadString(Constants.ANNOUNCEMENT_MAXLEN);
            }

            EndOfList = reader.ReadBoolean();
            reader.ReadBytes(3);
        }

        public override void Serialize(MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            //
            writer.Write(MessageID ?? MessageId.Empty);

            // 
            writer.Write(new byte[3]);
            writer.Write(StatusCode);
            writer.Write(AnnouncementID);

            
            // && writer.AppId == 24000 && writer.AppId == 24180 && writer.AppId == 20095 && writer.AppId != 22920 && writer.AppId != 21743 && writer.AppId == 10984
            if (writer.MediusVersion <= 112)
            {
                writer.Write(Announcement, Constants.ANNOUNCEMENT_MAXLEN);
            } else if (writer.MediusVersion == 113 /*&& writer.AppId == 21731 && writer.AppId == 21741 && writer.AppId == 22920 && writer.AppId == 21834 && writer.AppId == 20174*/) {
                writer.Write(Announcement, Constants.ANNOUNCEMENT1_MAXLEN);
            } else {
                writer.Write(Announcement, Constants.ANNOUNCEMENT_MAXLEN);
            }
            writer.Write(EndOfList);
            writer.Write(new byte[3]);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"StatusCode: {StatusCode} " +
                $"AnnouncementID: {AnnouncementID} " +
                $"Announcement: {Announcement} " +
                $"EndOfList: {EndOfList}";
        }
    }
}