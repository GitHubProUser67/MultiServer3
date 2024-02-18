using BackendProject.Horizon.RT.Common;
using BackendProject.Horizon.LIBRARY.Common.Stream;

namespace BackendProject.Horizon.RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_SERVER_APP)]
    public class RT_MSG_SERVER_APP : BaseScertMessage
    {
        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_SERVER_APP;

        public BaseMediusMessage? Message { get; set; } = null;
        //public BaseMediusGHSMessage GHSMessage { get; set; } = null;
        public override bool SkipEncryption
        {
            get => Message?.SkipEncryption ?? base.SkipEncryption;
            set
            {
                if (Message != null) { Message.SkipEncryption = value; }
                base.SkipEncryption = value;
            }
        }

        public override void Deserialize(MessageReader reader)
        {
            Message = BaseMediusMessage.Instantiate(reader);
            /*
            if(reader.AppId == 0)
            {
                //GHSMessage = BaseMediusGHSMessage.Instantiate(reader);
            } else
            {

            }
            */
        }

        public override void Serialize(MessageWriter writer)
        {
            writer.Write(Message.PacketClass);
            writer.Write(Message.PacketType);
            Message.Serialize(writer);
            /*
            if (Message != null)
            {
            } else
            {
                writer.Write(GHSMessage.msgSize);
                writer.Write(ReverseBytes16((ushort)GHSMessage.GhsOpcode));
                GHSMessage.Serialize(writer);
            }
            */
        }

        public override bool CanLog()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Message: {Message}";
            /*
            if(Message != null)
            {
            } else {
                return base.ToString() + " " +
                    $"GHSMessage: {GHSMessage}";
            }
            */
        }
         /// <summary>
         /// Reverses UInt16 
         /// </summary>
         /// <param name="nValue"></param>
         /// <returns></returns>
        public static ushort ReverseBytes16(ushort nValue)
        {
            return (ushort)((ushort)((nValue >> 8)) | (nValue << 8));
        }
    }
}