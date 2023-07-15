using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Common.Stream;

namespace PSMultiServer.SRC_Addons.MEDIUS.RT.Models
{
    public class RawMediusMessage : BaseMediusMessage
    {

        protected NetMessageClass _class;
        public override NetMessageClass PacketClass => _class;

        protected byte _messageType;
        public override byte PacketType => _messageType;

        public byte[] Contents { get; set; }

        public RawMediusMessage()
        {

        }

        public RawMediusMessage(NetMessageClass msgClass, byte messageType)
        {
            _class = msgClass;
            _messageType = messageType;
        }

        public override void Deserialize(MessageReader reader)
        {
            Contents = reader.ReadRest();
        }

        public override void Serialize(MessageWriter writer)
        {
            if (Contents != null)
                writer.Write(Contents);
        }

        public override string ToString()
        {
            return base.ToString() + $" MsgClass:{PacketClass} MsgType:{PacketType} Contents:{BitConverter.ToString(Contents)}";
        }
    }


    public class RawMediusClientMessage : BaseMediusPluginMessage
    {
        protected byte _size;
        public override byte Size => _size;


        protected NetMessageTypeIds _messageType;
        public override NetMessageTypeIds PacketType => _messageType;

        public byte[] Contents { get; set; }

        public override byte IncomingMessage => throw new NotImplementedException();

        public override byte PluginId => throw new NotImplementedException();

        public RawMediusClientMessage()
        {

        }

        public RawMediusClientMessage(byte size, NetMessageTypeIds messageType)
        {
            _size = size;
            _messageType = messageType;
        }

        public override void DeserializePlugin(MessageReader reader)
        {
            Contents = reader.ReadRest();
        }

        public override void SerializePlugin(MessageWriter writer)
        {
            if (Contents != null)
                writer.Write(Contents);
        }

        public override string ToString()
        {
            return base.ToString() + $" MsgType: {PacketType} Contents: {BitConverter.ToString(Contents)}";
        }
    }

    public class RawMediusServerMessage : BaseMediusPluginMessage
    {
        protected byte _incomingMessage;
        public override byte IncomingMessage => _incomingMessage;

        protected byte _size;
        public override byte Size => _size;

        protected byte _pluginId;
        public override byte PluginId => _pluginId;


        protected NetMessageTypeIds _messageType;
        public override NetMessageTypeIds PacketType => _messageType;

        public byte[] Contents { get; set; }

        public RawMediusServerMessage()
        {

        }

        public RawMediusServerMessage(byte incomingMesg, byte size, byte PluginId, NetMessageTypeIds messageType)
        {
            _pluginId = PluginId;
            _incomingMessage = incomingMesg;
            _size = size;
            _messageType = messageType;
        }

        public override void DeserializePlugin(MessageReader reader)
        {
            Contents = reader.ReadRest();
        }

        public override void SerializePlugin(MessageWriter writer)
        {
            if (Contents != null)
                writer.Write(Contents);
        }

        public override string ToString()
        {
            return base.ToString() + $" MsgType: {PacketType} Contents: {BitConverter.ToString(Contents)}";
        }
    }
    /*
    public class RawGHSMediusMessage : BaseMediusGHSMessage
    {
        protected ushort _msgSize;
        public override ushort msgSize => _msgSize;

        protected GhsOpcode _messageType;
        public override GhsOpcode GhsOpcode => _messageType;

        public byte[] Contents { get; set; }

        public RawGHSMediusMessage()
        {

        }

        public RawGHSMediusMessage(GhsOpcode messageType, ushort msgSize)
        {
            _messageType = (GhsOpcode)ReverseBytes16((ushort)messageType);
            _msgSize = msgSize;
        }

        public override void Deserialize(MessageReader reader)
        {
            Contents = reader.ReadRest();
        }

        public override void Serialize(MessageWriter writer)
        {
            if (Contents != null)
                writer.Write(Contents);
        }

        public override string ToString()
        {
            return base.ToString() + $" MsgType: {GhsOpcode} Contents: {BitConverter.ToString(Contents)}";
        } 
        /// <summary>
          /// Reverses UInt16 
          /// </summary>
          /// <param name="nValue"></param>
          /// <returns></returns>
        public static new ushort ReverseBytes16(ushort nValue)
        {
            return (ushort)((ushort)((nValue >> 8)) | (nValue << 8));
        }
    }
    */
}
