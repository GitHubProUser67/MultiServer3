using Horizon.RT.Common;
using Horizon.LIBRARY.Common.Stream;

namespace Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobby, MediusLobbyMessageIds.FileUploadResponse)]
    public class MediusFileUploadResponse : BaseLobbyMessage, IMediusResponse
    {

        public override byte PacketType => (byte)MediusLobbyMessageIds.FileUploadResponse;

        public bool IsSuccess => StatusCode >= 0;

        public MessageId MessageID { get; set; }

        public byte[] Data = new byte[Constants.MEDIUS_FILE_MAX_DOWNLOAD_DATA_SIZE];
        public int iStartByteIndex;
        public int iDataSize;
        public int iPacketNumber;
        public MediusFileXferStatus iXferStatus;
        public MediusCallbackStatus StatusCode;

        public override void Deserialize(MessageReader reader)
        {
            // 
            Data = reader.ReadBytes(Constants.MEDIUS_FILE_MAX_DOWNLOAD_DATA_SIZE);
            iStartByteIndex = reader.ReadInt32();
            iDataSize = reader.ReadInt32();
            iPacketNumber = reader.ReadInt32();
            iXferStatus = reader.Read<MediusFileXferStatus>();
            StatusCode = reader.Read<MediusCallbackStatus>();

            // 
            base.Deserialize(reader);

            //
            MessageID = reader.Read<MessageId>();
            reader.ReadBytes(3);
        }

        public override void Serialize(MessageWriter writer)
        {
            // 
            writer.Write(Data, Constants.MEDIUS_FILE_MAX_DOWNLOAD_DATA_SIZE);
            writer.Write(iStartByteIndex);
            writer.Write(iDataSize);
            writer.Write(iPacketNumber);
            writer.Write(iXferStatus);
            writer.Write(StatusCode);

            // 
            base.Serialize(writer);

            //
            writer.Write(MessageID ?? MessageId.Empty);
            writer.Write(new byte[3]);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"Data: {(Data != null ? BitConverter.ToString(Data) : "<null>")} " +
                $"iStartByteIndex: {iStartByteIndex} " +
                $"iDataSize: {iDataSize} " +
                $"iPacketNumber: {iPacketNumber} " +
                $"iXferStatus: {iXferStatus} " +
                $"StatusCode: {StatusCode}";
        }
    }
}