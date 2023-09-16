using System.Text;

namespace MultiServer.CryptoSporidium
{
    public class LIBSECURE
    {
        public static string MemXOR(string IV, string block, int blocksize) // Cannot handle non-parity.
        {
            StringBuilder CryptoBytes = new StringBuilder();

            if (blocksize == 8)
            {
                for (int i = 4; i != 0; --i)
                {
                    string BlockIV = IV.Substring(0, 4);
                    string CipherBlock = block.Substring(0, 4);
                    IV = IV.Substring(4);
                    block = block.Substring(4);
                    ushort uBlockIV = Convert.ToUInt16(BlockIV, 16);
                    ushort uCipherBlock = Convert.ToUInt16(CipherBlock, 16);

                    ushort Xor = (ushort)(uBlockIV ^ uCipherBlock);
                    string output = Xor.ToString("X4");
                    try
                    {
                        CryptoBytes.Append(Misc.ByteArrayToHexString(Misc.HexStringToByteArray(output)));
                    }
                    catch (Exception ex)
                    {
                        ServerConfiguration.LogError($"[LIBSECURE] - Error In MemXOR: {ex}");
                    }
                }
            }
            else if (blocksize == 16)
            {
                for (int i = 4; i != 0; --i)
                {
                    string BlockIV = IV.Substring(0, 8);
                    string CipherBlock = block.Substring(0, 8);
                    IV = IV.Substring(8);
                    block = block.Substring(8);
                    uint uBlockIV = Convert.ToUInt32(BlockIV, 16);
                    uint uCipherBlock = Convert.ToUInt32(CipherBlock, 16);

                    uint Xor = (uint)(uBlockIV ^ uCipherBlock);
                    string output = Xor.ToString("X8");
                    try
                    {
                        CryptoBytes.Append(Misc.ByteArrayToHexString(Misc.HexStringToByteArray(output)));
                    }
                    catch (Exception ex)
                    {
                        ServerConfiguration.LogError($"[LIBSECURE] - Error In MemXOR: {ex}");
                    }
                }
            }

            return CryptoBytes.ToString();
        }
    }
}
