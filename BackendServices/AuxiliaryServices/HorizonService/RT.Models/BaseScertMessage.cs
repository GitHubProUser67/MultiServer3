using CustomLogger;
using Horizon.RT.Common;
using Horizon.RT.Cryptography;
using Horizon.LIBRARY.Common.Stream;

namespace Horizon.RT.Models
{
    public abstract class BaseScertMessage
    {
        public const int HEADER_SIZE = 3;
        public const int HASH_SIZE = 4;

        /// <summary>
        /// Message id.
        /// </summary>
        public abstract RT_MSG_TYPE Id { get; }

        /// <summary>
        /// When true, skips encryption when sending this particular message instance.
        /// </summary>
        public virtual bool SkipEncryption { get; set; } = false;

        public BaseScertMessage()
        {

        }

        #region Serialization

        /// <summary>
        /// Deserializes the message from plaintext.
        /// </summary>
        /// <param name="reader"></param>
        public abstract void Deserialize(MessageReader reader);

        /// <summary>
        /// Serializes the message.
        /// </summary>
        public List<byte[]> Serialize(int? mediusVersion, int appId, CipherService cipherService)
        {
            var results = new List<byte[]>();
            byte[]? result = null;
            var buffer = new byte[1024 * 10];
            int length = 0;
            int totalHeaderSize = HEADER_SIZE;

            // Serialize message
            using (var stream = new MemoryStream(buffer, true))
            {
                using (var writer = new MessageWriter(stream) { MediusVersion = (int)mediusVersion, AppId =  appId})
                {
                    Serialize(writer);
                    length = (int)writer.BaseStream.Position;
                }
            }

            var ctx = (Id == RT_MSG_TYPE.RT_MSG_SERVER_CRYPTKEY_PEER || Id == RT_MSG_TYPE.RT_MSG_CLIENT_CRYPTKEY_PUBLIC) ? CipherContext.RSA_AUTH : CipherContext.RC_CLIENT_SESSION;

            // Check for fragmentation
            if (Id == RT_MSG_TYPE.RT_MSG_SERVER_APP && length > Constants.MEDIUS_MESSAGE_MAXLEN && mediusVersion != 108 && appId != 21834)
            {
                var msgClass = (NetMessageClass)buffer[0];
                var msgType = buffer[1];
                var fragments = TypePacketFragment.FromPayload(msgClass, msgType, buffer, 2, length - 2);

                foreach (var frag in fragments)
                {
                    totalHeaderSize = HEADER_SIZE;

                    // Serialize message
                    using (var stream = new MemoryStream(buffer, true))
                    {
                        using (var writer = new MessageWriter(stream))
                        {
                            // Serialize message
                            new RT_MSG_SERVER_APP() { Message = frag }.Serialize(writer);
                            length = (int)stream.Position;

                            var data = new byte[length];
                            Array.Copy(buffer, data, length);
                            if (!SkipEncryption && cipherService != null && cipherService.Encrypt(ctx, data, out var signed, out var hash))
                            {
                                totalHeaderSize += HASH_SIZE;

                                Array.Copy(hash, 0, buffer, 3, hash.Length);
                                Array.Copy(signed, 0, buffer, 3 + hash.Length, signed.Length);
                                writer.Seek(0, SeekOrigin.Begin);
                                writer.Write((byte)((byte)Id | 0x80));
                            }
                            else
                            {
                                Array.Copy(buffer, 0, buffer, 3, length);
                                writer.Seek(0, SeekOrigin.Begin);
                                writer.Write((byte)Id);
                            }

                            // Write length
                            writer.Seek(1, SeekOrigin.Begin);
                            writer.Write((ushort)length);

                            // 
                            result = new byte[length + totalHeaderSize];
                            Array.Copy(buffer, 0, result, 0, result.Length);
                            results.Add(result);
                        }
                    }
                }
            }
            else
            {
                var data = new byte[length];
                Array.Copy(buffer, data, length);
                if (!SkipEncryption && cipherService != null && cipherService.Encrypt(ctx, data, out var signed, out var hash))
                {
                    totalHeaderSize += HASH_SIZE;

                    result = new byte[length + totalHeaderSize];
                    result[0] = (byte)((byte)Id | 0x80);
                    result[1] = (byte)(length & 0xFF);
                    result[2] = (byte)((length >> 8) & 0xFF);
                    Array.Copy(hash, 0, result, HEADER_SIZE, HASH_SIZE);
                    Array.Copy(signed, 0, result, totalHeaderSize, length);
                }
                else
                {
                    // Add id and length to header
                    result = new byte[length + totalHeaderSize];
                    result[0] = (byte)Id;
                    result[1] = (byte)(length & 0xFF);
                    result[2] = (byte)((length >> 8) & 0xFF);
                    Array.Copy(buffer, 0, result, totalHeaderSize, length);
                }

                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// Serialize contents of the message.
        /// </summary>
        public abstract void Serialize(MessageWriter writer);

        #endregion

        #region Logging

        /// <summary>
        /// Whether or not this message passes the log filter.
        /// </summary>
        public virtual bool CanLog()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }

        #endregion

        #region Dynamic Instantiation

        private static Dictionary<RT_MSG_TYPE, Type>? _messageClassById = null;
        private static int _messageClassByIdLockValue = 0;
        private static object _messageClassByIdLockObject = _messageClassByIdLockValue;

        private static void Initialize()
        {
            lock (_messageClassByIdLockObject)
            {
                if (_messageClassById != null)
                    return;

                _messageClassById = new Dictionary<RT_MSG_TYPE, Type>();

                // Populate
                var assembly = System.Reflection.Assembly.GetAssembly(typeof(BaseScertMessage));
                var types = assembly.GetTypes();

                foreach (Type classType in types)
                {
                    // Objects by Id
                    var attrs = (ScertMessageAttribute[])classType.GetCustomAttributes(typeof(ScertMessageAttribute), true);
                    if (attrs != null && attrs.Length > 0)
                        _messageClassById.Add(attrs[0].MessageId, classType);
                }
            }
        }

        public static void RegisterMessage(RT_MSG_TYPE id, Type type)
        {
            // Init first
            Initialize();

            // Set or overwrite.
            if (!_messageClassById.ContainsKey(id))
                _messageClassById.Add(id, type);
            else
                _messageClassById[id] = type;
        }

        public static BaseScertMessage Instantiate(MessageReader reader)
        {
            var id = reader.ReadByte();
            var rtId = (RT_MSG_TYPE)(id & 0x7f);
            var len = reader.ReadInt16();
            var messageBytes = reader.ReadBytes(len);
            if (id >= 0x80)
                throw new Exception($"Unable instantiate encrypted message {id} without a cipher!");


            return Instantiate(rtId, null, messageBytes, reader.MediusVersion, reader.AppId, null);
        }

        public static BaseScertMessage? Instantiate(RT_MSG_TYPE id, byte[]? hash, byte[] messageBuffer, int mediusVersion, int appId, CipherService? cipherService)
        {
            // Init first
            Initialize();

            BaseScertMessage? msg = null;

            // Get class
            if (!_messageClassById.TryGetValue(id, out var classType))
                classType = null;

            // Decrypt
            if (hash != null)
            {
                if (cipherService.Decrypt(messageBuffer, hash, out var plain))
                    msg = Instantiate(classType, id, plain, mediusVersion, appId);
                else
                    LoggerAccessor.LogError($"Unable to decrypt {id}, HASH:{BitConverter.ToString(hash)} DATA:{BitConverter.ToString(messageBuffer).Replace("-", "")}");
            }
            else
                msg = Instantiate(classType, id, messageBuffer, mediusVersion, appId);

            return msg;
        }

        private static BaseScertMessage? Instantiate(Type classType, RT_MSG_TYPE id, byte[] plain, int mediusVersion, int appId)
        {
            if (plain == null)
            {
                LoggerAccessor.LogError("[BaseScertMessage-Instantiate] - null plain given to function!");
                return null;
            }

            BaseScertMessage? msg = null;

            using (var stream = new MemoryStream(plain))
            {
                using (var reader = new MessageReader(stream) { MediusVersion = mediusVersion, AppId = appId })
                {
                    if (classType == null)
                        msg = new RawScertMessage(id);
                    else
                        msg = (BaseScertMessage)Activator.CreateInstance(classType);

                    try
                    {
                        msg.Deserialize(reader);
                    }
                    catch (Exception e)
                    {
                        LoggerAccessor.LogError($"Error deserializing {id} {BitConverter.ToString(plain)}");
                        LoggerAccessor.LogError(e);
                    }
                }
            }

            return msg;
        }

        #endregion

        public override string ToString()
        {
            return $"Id: {Id}";
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ScertMessageAttribute : Attribute
    {
        public RT_MSG_TYPE MessageId;

        public ScertMessageAttribute(RT_MSG_TYPE id)
        {
            MessageId = id;
        }
    }
}