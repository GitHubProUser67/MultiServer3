using PSMultiServer.Addons.Medius.RT.Common;
using PSMultiServer.Addons.Medius.Server.Common.Stream;

namespace PSMultiServer.Addons.Medius.RT.Models
{

    #region BaseMediusPluginMessage
    public abstract class BaseMediusPluginMessage
    {
        /// <summary>
        /// Message class.
        /// </summary>
        public abstract byte IncomingMessage { get; }

        public abstract byte Size { get; }

        public abstract byte PluginId { get; }

        /// <summary>
        /// Message type.
        /// </summary>
        public abstract NetMessageTypeIds PacketType { get; }

        /// <summary>
        /// When true, skips encryption when sending this particular message instance.
        /// </summary>
        public virtual bool SkipEncryption { get; set; } = false;

        public BaseMediusPluginMessage()
        {

        }

        #region Serialization

        /// <summary>
        /// Deserializes the plugin message from plaintext.
        /// </summary>
        /// <param name="reader"></param>
        public virtual void DeserializePlugin(MessageReader reader)
        {

        }

        /// <summary>
        /// Serialize contents of the plugin message.
        /// </summary>
        public virtual void SerializePlugin(MessageWriter writer)
        {

        }

        #endregion

        #region Logging
        /*
        /// <summary>
        /// Whether or not this message passes the log filter.
        /// </summary>
        public virtual bool CanLog()
        {
            
        }
        */
        #endregion

        #region Dynamic Instantiation

        private static Dictionary<NetMessageTypeIds, Type> _netPluginMessageTypeById = null;

        private static int _messageClassByIdLockValue = 0;
        private static object _messageClassByIdLockObject = _messageClassByIdLockValue;


        private static void Initialize()
        {
            lock (_messageClassByIdLockObject)
            {

                _netPluginMessageTypeById = new Dictionary<NetMessageTypeIds, Type>();

                // Populate
                var assembly = System.Reflection.Assembly.GetAssembly(typeof(BaseMediusPluginMessage));
                var types = assembly.GetTypes();


                foreach (Type classType in types)
                {
                    // Objects by Id
                    var attrs = (MediusMessageAttribute[])classType.GetCustomAttributes(typeof(MediusMessageAttribute), true);
                    if (attrs != null && attrs.Length > 0)
                    {
                        switch (attrs[0].MessageClass)
                        {
                            case NetMessageClass.MessageClassApplication:
                                {
                                    _netPluginMessageTypeById.Add((NetMessageTypeIds)attrs[0].MessageType, classType);
                                    break;
                                }
                        }

                    }
                }
            }
        }

        public static BaseMediusPluginMessage InstantiateClientPlugin(MessageReader reader)
        {
            BaseMediusPluginMessage msg;

            Type classType = null;

            var msgSize = reader.ReadByte();

            //reader.ReadBytes(2);
            //var msgSize = reader.ReadUInt16();
            var msgTypeBW = reader.Read<NetMessageTypeIds>();


            var msgTypeByteInt = Convert.ToInt32(msgTypeBW);
            var msgTypeReversed = ReverseBytesInt(msgTypeByteInt);
            //var reversedMsgType = Reverse(Convert.ToInt32(msgTypeBW));


            NetMessageTypeIds msgType = (NetMessageTypeIds)msgTypeReversed;

            // Init
            Initialize();

            if (!_netPluginMessageTypeById.TryGetValue(msgType, out classType))
                classType = null;

            // Instantiate
            if (classType == null)
                msg = new RawMediusClientMessage(msgSize, msgType);
            else
                msg = (BaseMediusPluginMessage)Activator.CreateInstance(classType);

            // Deserialize
            msg.DeserializePlugin(reader);
            return msg;
        }


        public static BaseMediusPluginMessage InstantiateServerPlugin(MessageReader reader)
        {
            BaseMediusPluginMessage msg;

            Type classType = null;

            var incomingMsg = reader.ReadByte();
            reader.ReadByte();
            var msgSize = reader.ReadByte();
            var PluginId = reader.ReadByte();

            var msgSizeReversed = ReverseBytesInt(Convert.ToInt32(msgSize));

            var msgTypeBW = reader.Read<NetMessageTypeIds>();

            var msgTypeByteInt = Convert.ToInt32(msgTypeBW);
            var msgTypeReversed = ReverseBytesInt(msgTypeByteInt);
            //var reversedMsgType = Reverse(Convert.ToInt32(msgTypeBW));


            NetMessageTypeIds msgType = (NetMessageTypeIds)msgTypeReversed;

            // Init
            Initialize();

            if (!_netPluginMessageTypeById.TryGetValue(msgType, out classType))
                classType = null;

            // Instantiate
            if (classType == null)
                msg = new RawMediusServerMessage(incomingMsg, Convert.ToByte(msgSizeReversed), PluginId, msgType);
            else
                msg = (BaseMediusPluginMessage)Activator.CreateInstance(classType);

            // Deserialize
            msg.DeserializePlugin(reader);
            return msg;
        }

        #endregion

        public static uint ReverseBytesUInt(uint value)
        {
            return (uint)((value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24);
        }

        public static int ReverseBytesInt(int value)
        {
            return (int)((value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24);
        }
    }
    #endregion
}
