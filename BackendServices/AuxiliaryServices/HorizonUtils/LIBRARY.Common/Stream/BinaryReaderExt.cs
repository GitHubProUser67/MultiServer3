using System.Net;
using System.Text;

namespace Horizon.LIBRARY.Common.Stream
{
    public static class BinaryReaderExt
    {

        public static T Read<T>(this BinaryReader reader)
        {
            var result = reader.ReadObject(typeof(T));

            return result == null ? default(T) : (T)result;
        }

        public static IPAddress ReadIPAddress(this BinaryReader reader)
        {
            return IPAddress.Parse(reader.ReadString(16));
        }

        public static string ReadString(this BinaryReader reader, int length)
        {
            byte[] buffer = reader.ReadBytes(length);
            int i = 0;
            for (i = 0; i < buffer.Length; ++i)
                if (buffer[i] == 0)
                    break;

            if (i > 0)
                return Encoding.UTF8.GetString(buffer, 0, i);
            else
                return string.Empty;
        }

        public static object ReadObject(this BinaryReader reader, Type type)
        {
            if (type.GetInterface("IStreamSerializer") != null)
            {
                var result = (IStreamSerializer)Activator.CreateInstance(type);
                result.Deserialize(reader);
                return result;
            }
            else if (type.IsEnum)
                return reader.ReadObject(type.GetEnumUnderlyingType()); //Enum.Parse(type, type.GetEnumName(reader.ReadObject(type.GetEnumUnderlyingType())));
            else if (type == typeof(bool))
                return reader.ReadBoolean();
            else if (type == typeof(byte))
                return reader.ReadByte();
            else if (type == typeof(sbyte))
                return reader.ReadSByte();
            else if (type == typeof(char))
                return reader.ReadChar();
            else if (type == typeof(short))
                return reader.ReadInt16();
            else if (type == typeof(ushort))
                return reader.ReadUInt16();
            else if (type == typeof(int))
                return reader.ReadInt32();
            else if (type == typeof(uint))
                return reader.ReadUInt32();
            else if (type == typeof(long))
                return reader.ReadInt64();
            else if (type == typeof(ulong))
                return reader.ReadUInt64();
            else if (type == typeof(float))
                return reader.ReadSingle();
            else if (type == typeof(double))
                return reader.ReadDouble();
            else if (type == typeof(string))
                return reader.ReadString();

            return null;
        }

        public static byte[] ReadRest(this BinaryReader reader)
        {
            return reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
        }

        public static string ReadRestAsString(this BinaryReader reader)
        {
            return reader.ReadString((int)(reader.BaseStream.Length - reader.BaseStream.Position));
        }
    }
}