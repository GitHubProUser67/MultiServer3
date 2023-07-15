using Org.BouncyCastle.Crypto.Digests;

namespace PSMultiServer.SRC_Addons.MEDIUS.RT.Cryptography.Hash
{
    public static class SHA1
    {
        public static byte[] Hash(byte[] input, CipherContext context)
        {
            byte[] result = new byte[4];
            Hash(input, 0, input.Length, result, 0, (byte)context);
            return result;
        }

        private static void Hash(
            byte[] input,
                int inOff,
                int length,
                byte[] output,
                int outOff,
                byte encryptionType)
        {
            byte[] result = new byte[20];

            // Compute sha1 hash
            Sha1Digest digest = new Sha1Digest();
            digest.BlockUpdate(input, inOff, length);
            digest.DoFinal(result, 0);

            // Inject context inter highest 3 bits
            result[3] = (byte)((result[3] & 0x1F) | ((encryptionType & 7) << 5));

            Array.Copy(result, 0, output, outOff, 4);
        }
    }
}
