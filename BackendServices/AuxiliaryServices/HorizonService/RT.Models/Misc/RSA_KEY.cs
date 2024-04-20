using System.IO;
using Horizon.RT.Common;
using Horizon.LIBRARY.Common.Stream;
using System.Linq;

namespace Horizon.RT.Models
{
    public class RSA_KEY : IStreamSerializer
    {
        public readonly static RSA_KEY Empty = new RSA_KEY();

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
            return string.Join(string.Empty, key?.Select(x => x.ToString("X8")));
        }
    }
}
