using System;

namespace HomeTools.Crypto
{
    public class ProfanitySecurity
    {
        private static readonly uint[] ProfanityFilterKey = new uint[4]
        {
            0xF512A417, 0x485EF87A, 0xB3D85E90, 0xC4923F75
        };

        public static byte[] PF_DeCipher(byte[] FileBytes)
        {
            // Offset: 0001d234 in 1.82 symbols.

            throw new NotImplementedException("Pre-1.86 ProfanityFilter Decryption not supported yet!");
        }
    }
}
