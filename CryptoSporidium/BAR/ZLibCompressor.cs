using MultiServer.Addons.ComponentAce.Compression.Libs.zlib;

namespace MultiServer.CryptoSporidium.BAR
{
    internal static class ZLibCompressor
    {
        internal static byte[] Compress(byte[] inData, bool NoHeader)
        {
            byte[] result = null;
            MemoryStream memoryStream = new MemoryStream();
            ZOutputStream zoutputStream = new ZOutputStream(memoryStream, 9, NoHeader);
            try
            {
                zoutputStream.Write(inData, 0, inData.Length);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                zoutputStream.Close();
                memoryStream.Close();
                result = memoryStream.ToArray();
            }
            return result;
        }

        internal static byte[] Decompress(byte[] inData, bool NoHeader)
        {
            MemoryStream memoryStream = new MemoryStream();
            ZOutputStream zoutputStream = new ZOutputStream(memoryStream, NoHeader);
            zoutputStream.Write(inData, 0, inData.Length);
            zoutputStream.Close();
            memoryStream.Close();
            return memoryStream.ToArray();
        }
    }
}
