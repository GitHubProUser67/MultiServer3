using CustomLogger;
using CryptoSporidium.Horizon.RT.Common;
using CryptoSporidium.Horizon.LIBRARY.Common.Stream;
using System.Globalization;

namespace CryptoSporidium.Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobbyExt, MediusLobbyExtMessageIds.UniverseVariableInformationResponse)]
    public class MediusUniverseVariableInformationResponse : BaseLobbyExtMessage, IMediusResponse
    {
        public override byte PacketType => (byte)MediusLobbyExtMessageIds.UniverseVariableInformationResponse;

        public bool IsSuccess => StatusCode >= 0;

        public MessageId? MessageID { get; set; }

        public MediusCallbackStatus StatusCode;
        public MediusUniverseVariableInformationInfoFilter InfoFilter;
        public uint UniverseID;
        public string? UniverseName; // UNIVERSENAME_MAXLEN
        public string? DNS; // UNIVERSEDNS_MAXLEN
        public int Port;
        public string? UniverseDescription; // UNIVERSEDESCRIPTION_MAXLEN
        public int Status;
        public int UserCount;
        public int MaxUsers;
        public string? UniverseBilling; // UNIVERSE_BSP_MAXLEN
        public string? BillingSystemName; // UNIVERSE_BSP_NAME_MAXLEN
        public string? ExtendedInfo; // UNIVERSE_EXTENDED_INFO_MAXLEN
        public string? SvoURL; // UNIVERSE_SVO_URL_MAXLEN
        public bool EndOfList;

        public List<int> approvedList = new()
        { 
            10421,
            10994,
            20464,
            21093, 
            21614, 
            21624,
            21834,
            20371,
            20374,
            20244,
            21094,
            21324, 
            21514,
            21784,
            22073, 
            20464, 
            22500,
            22920,
            21694, 
            22930, 
            22924,
            50041,
            50083,
            50089,
            50097,
            50098,
            50100,
            50121,
            50130,
            50132,
            50135,
            50141,
            50160,
            50161,
            50162,
            50165,
            50170,
            50175,
            50182,
            50183,
            50185,
            50186
        };

        public override void Deserialize(MessageReader reader)
        {
            base.Deserialize(reader);

            MessageID = reader.Read<MessageId>();

            StatusCode = reader.Read<MediusCallbackStatus>();
            InfoFilter = reader.Read<MediusUniverseVariableInformationInfoFilter>();

            if (InfoFilter.IsSet(MediusUniverseVariableInformationInfoFilter.INFO_ID))
                UniverseID = reader.ReadUInt32();

            if (InfoFilter.IsSet(MediusUniverseVariableInformationInfoFilter.INFO_NAME))
                UniverseName = reader.ReadString(Constants.UNIVERSENAME_MAXLEN);

            if (InfoFilter.IsSet(MediusUniverseVariableInformationInfoFilter.INFO_DNS))
            {
                DNS = reader.ReadString(Constants.UNIVERSEDNS_MAXLEN);
                Port = reader.ReadInt32();
            }

            if (InfoFilter.IsSet(MediusUniverseVariableInformationInfoFilter.INFO_DESCRIPTION))
                UniverseDescription = reader.ReadString(Constants.UNIVERSEDESCRIPTION_MAXLEN);

            if (InfoFilter.IsSet(MediusUniverseVariableInformationInfoFilter.INFO_STATUS))
            {
                Status = reader.ReadInt32();
                UserCount = reader.ReadInt32();
                MaxUsers = reader.ReadInt32();
            }

            if (InfoFilter.IsSet(MediusUniverseVariableInformationInfoFilter.INFO_BILLING))
            {
                UniverseBilling = reader.ReadString(Constants.UNIVERSE_BSP_MAXLEN);
                BillingSystemName = reader.ReadString(Constants.UNIVERSE_BSP_NAME_MAXLEN);
            }

            if (InfoFilter.IsSet(MediusUniverseVariableInformationInfoFilter.INFO_EXTRAINFO))
                ExtendedInfo = reader.ReadString(Constants.UNIVERSE_EXTENDED_INFO_MAXLEN);

            //if (InfoFilter.IsSet(MediusUniverseVariableInformationInfoFilter.INFO_SVO_URL))
            //    SvoURL = reader.ReadString(Constants.UNIVERSE_SVO_URL_MAXLEN);

            EndOfList = reader.ReadBoolean();

        }

        public override void Serialize(MessageWriter writer)
        {
            base.Serialize(writer);

            writer.Write(MessageID ?? MessageId.Empty);

            //writer.Write(new byte[3]);

            writer.Write(StatusCode);
            writer.Write(InfoFilter);
            
            if (InfoFilter.IsSet(MediusUniverseVariableInformationInfoFilter.INFO_ID))
                writer.Write(UniverseID);

            if (InfoFilter.IsSet(MediusUniverseVariableInformationInfoFilter.INFO_NAME))
                writer.Write(UniverseName, Constants.UNIVERSENAME_MAXLEN);

            if (InfoFilter.IsSet(MediusUniverseVariableInformationInfoFilter.INFO_DNS))
            {
                writer.Write(DNS, Constants.UNIVERSEDNS_MAXLEN);
                writer.Write(Port);
            }

            if (InfoFilter.IsSet(MediusUniverseVariableInformationInfoFilter.INFO_DESCRIPTION))
                writer.Write(UniverseDescription, Constants.UNIVERSEDESCRIPTION_MAXLEN);

            if (InfoFilter.IsSet(MediusUniverseVariableInformationInfoFilter.INFO_STATUS))
            {
                writer.Write(Status);
                writer.Write(UserCount);
                writer.Write(MaxUsers);
            }

            if (InfoFilter.IsSet(MediusUniverseVariableInformationInfoFilter.INFO_BILLING))
            {
                writer.Write(UniverseBilling, Constants.UNIVERSE_BSP_MAXLEN);
                writer.Write(BillingSystemName, Constants.UNIVERSE_BSP_NAME_MAXLEN);
            }

            if (InfoFilter.IsSet(MediusUniverseVariableInformationInfoFilter.INFO_EXTRAINFO))
                writer.Write(ExtendedInfo, Constants.UNIVERSE_EXTENDED_INFO_MAXLEN);

            if (approvedList.Contains(writer.AppId))
            {
                switch (writer.AppId)
                {
                    case 20371:
                    case 20374:
                    case 50041:
                    case 50083:
                    case 50089:
                    case 50097:
                    case 50098:
                    case 50100:
                    case 50121:
                    case 50130:
                    case 50132:
                    case 50135:
                    case 50141:
                    case 50160:
                    case 50161:
                    case 50162:
                    case 50165:
                    case 50170:
                    case 50175:
                    case 50180:
                    case 50182:
                    case 50183:
                    case 50185:
                    case 50186:
                        double homever = 0;
                        string? firstFiveElements = null;
                        if (!string.IsNullOrEmpty(ExtendedInfo))
                            firstFiveElements = ExtendedInfo.Substring(0, Math.Min(5, ExtendedInfo.Length));

                        if (!string.IsNullOrEmpty(firstFiveElements))
                        {
                            try
                            {
                                homever = Double.Parse(firstFiveElements, CultureInfo.InvariantCulture);
                            }
                            catch (Exception)
                            {
                                homever = 0;
                            }
                        }

                        if (homever >= 01.21 || (!string.IsNullOrEmpty(firstFiveElements) && firstFiveElements[0] == '*'))
                        {
                            LoggerAccessor.LogInfo("[MediusUniverseVariableInformationResponse] - Setting SVOURL");
                            if (InfoFilter.IsSet(MediusUniverseVariableInformationInfoFilter.INFO_SVO_URL))
                                writer.Write(SvoURL, Constants.UNIVERSE_SVO_URL_MAXLEN);
                        }
                        else
                            LoggerAccessor.LogInfo("[MediusUniverseVariableInformationResponse] - Not writing SVOURL");
                        break;
                    default:
                        LoggerAccessor.LogInfo("[MediusUniverseVariableInformationResponse] - Setting SVOURL");
                        if (InfoFilter.IsSet(MediusUniverseVariableInformationInfoFilter.INFO_SVO_URL))
                            writer.Write(SvoURL, Constants.UNIVERSE_SVO_URL_MAXLEN);
                        break;
                }
            }
            else
                LoggerAccessor.LogInfo("[MediusUniverseVariableInformationResponse] - Not writing SVOURL");

            writer.Write(EndOfList);
            //writer.Write(new byte[3]);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"StatusCode: {StatusCode} " +
                $"InfoFilter: {InfoFilter} " +
                $"UniverseID: {UniverseID} " +
                $"UniverseName: {UniverseName} " +
                $"DNS: {DNS} " +
                $"Port: {Port} " +
                $"UniverseDescription: {UniverseDescription} " +
                $"Status: {Status} " +
                $"UserCount: {UserCount} " +
                $"MaxUsers: {MaxUsers} " +
                $"UniverseBilling: {UniverseBilling} " +
                $"BillingSystemName: {BillingSystemName} " +
                $"ExtendedInfo: {ExtendedInfo} " +
                $"SvoURL: {SvoURL} " +
                $"EndOfList: {EndOfList}";
        }
    }
}