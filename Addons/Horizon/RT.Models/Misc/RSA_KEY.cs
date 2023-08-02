using PSMultiServer.Addons.Horizon.RT.Common;
using PSMultiServer.Addons.Horizon.Server.Common.Stream;

namespace PSMultiServer.Addons.Horizon.RT.Models
{
    public class RSA_KEY : IStreamSerializer
    {
        public readonly static RSA_KEY Empty = new RSA_KEY();

        // 
        public uint[] key = new uint[Constants.RSA_SIZE_DWORD];

        public RSA_KEY()
        { }

        public RSA_KEY(byte[] keyBytes)
        {
            for (int i = 0; i < key.Length; i++)
            {
                key[i] = (uint)((keyBytes[(i * 4) + 3] << 24) | (keyBytes[(i * 4) + 2] << 16) | (keyBytes[(i * 4) + 1] << 8) | (keyBytes[(i * 4) + 0]));
            }
        }

        public void Deserialize(BinaryReader reader)
        {
            key = new uint[Constants.RSA_SIZE_DWORD];
            for (int i = 0; i < Constants.RSA_SIZE_DWORD; ++i)
                key[i] = reader.ReadUInt32();
        }

        public void Serialize(BinaryWriter writer)
        {
            for (int i = 0; i < Constants.RSA_SIZE_DWORD; ++i)
                writer.Write(i >= key.Length ? 0 : key[i]);
        }

        public override string ToString()
        {
            return string.Join("", key?.Select(x => x.ToString("X8")));
        }
    }
}