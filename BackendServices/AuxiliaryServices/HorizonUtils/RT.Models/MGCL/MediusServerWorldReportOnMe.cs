using Horizon.RT.Common;
using Horizon.LIBRARY.Common.Stream;

namespace Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobbyReport, MediusMGCLMessageIds.ServerWorldReportOnMe)]
    public class MediusServerWorldReportOnMe : BaseMGCLMessage, IMediusRequest
    {

        public override byte PacketType => (byte)MediusMGCLMessageIds.ServerWorldReportOnMe;

        public MessageId MessageID { get; set; }
        public string GameName; // MGCL_GAMENAME_MAXLEN
        public byte[] GameStats = new byte[Constants.MGCL_GAMESTATS_MAXLEN];
        public string GamePassword; // MGCL_GAMEPASSWORD_MAXLEN
        public int ApplicationID;
        public int MaxClients;
        public int MinClients;
        public int PlayerCount;
        public int GameLevel;
        public int PlayerSkillLevel;
        public int RulesSet;
        public int GenericField1;
        public int GenericField2;
        public int GenericField3;
        public int GenericField4;
        public int GenericField5;
        public int GenericField6;
        public int GenericField7;
        public int GenericField8;
        public int MediusWorldID;
        public MediusWorldStatus WorldStatus;

        public override void Deserialize(MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            // 
            MessageID = reader.Read<MessageId>();

            GameName = reader.ReadString(Constants.MGCL_GAMENAME_MAXLEN);
            GameStats = reader.ReadBytes(Constants.MGCL_GAMESTATS_MAXLEN);
            GamePassword = reader.ReadString(Constants.MGCL_GAMEPASSWORD_MAXLEN);
            reader.ReadBytes(3);
            ApplicationID = reader.ReadInt32();
            MaxClients = reader.ReadInt32();
            MinClients = reader.ReadInt32();
            PlayerCount = reader.ReadInt32();
            GameLevel = reader.ReadInt32();
            PlayerSkillLevel = reader.ReadInt32();
            RulesSet = reader.ReadInt32();
            GenericField1 = reader.ReadInt32();
            GenericField2 = reader.ReadInt32();
            GenericField3 = reader.ReadInt32();
            GenericField4 = reader.ReadInt32();
            GenericField5 = reader.ReadInt32();
            GenericField6 = reader.ReadInt32();
            GenericField7 = reader.ReadInt32();
            GenericField8 = reader.ReadInt32();
            MediusWorldID = reader.ReadInt32();
            WorldStatus = reader.Read<MediusWorldStatus>();
        }

        public override void Serialize(MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            // 
            writer.Write(MessageID ?? MessageId.Empty);
            writer.Write(GameName, Constants.MGCL_GAMENAME_MAXLEN);
            writer.Write(GameStats, Constants.MGCL_GAMESTATS_MAXLEN);
            writer.Write(GamePassword, Constants.MGCL_GAMEPASSWORD_MAXLEN);
            writer.Write(new byte[3]);
            writer.Write(ApplicationID);
            writer.Write(MaxClients);
            writer.Write(MinClients);
            writer.Write(GameLevel);
            writer.Write(PlayerSkillLevel);
            writer.Write(RulesSet);
            writer.Write(GenericField1);
            writer.Write(GenericField2);
            writer.Write(GenericField3);
            writer.Write(GenericField4);
            writer.Write(GenericField5);
            writer.Write(GenericField6);
            writer.Write(GenericField7);
            writer.Write(GenericField8);
            writer.Write(MediusWorldID);
            writer.Write(WorldStatus);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"GameName: {GameName} " +
                $"GameStats: {BitConverter.ToString(GameStats)} " +
                $"GamePassword: {GamePassword} " +
                $"ApplicationID: {ApplicationID} " +
                $"MaxClients: {MaxClients} " +
                $"MinClients: {MinClients} " +
                $"PlayerCount: {PlayerCount} " +
                $"GameLevel: {GameLevel} " +
                $"PlayerSkillLevel: {PlayerSkillLevel} " +
                $"RulesSet: {RulesSet} " +
                $"GenericField1: {GenericField1:X8} " +
                $"GenericField2: {GenericField2:X8} " +
                $"GenericField3: {GenericField3:X8} " +
                $"GenericField4: {GenericField4:X8} " +
                $"GenericField5: {GenericField5:X8} " +
                $"GenericField6: {GenericField6:X8} " +
                $"GenericField7: {GenericField7:X8} " +
                $"GenericField8: {GenericField8:X8} " +
                $"MediusWorldID: {MediusWorldID} " +
                $"WorldStatus: {WorldStatus} ";
        }
    }
}