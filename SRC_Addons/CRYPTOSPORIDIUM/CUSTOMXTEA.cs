using System.Text;

namespace PSMultiServer.Addons.CRYPTOSPORIDIUM
{
    public class CUSTOMXTEA
    {
        /// <summary>
		/// Libsecure documentation seems to recommand 64...
		/// </summary>
		private const uint Rounds = 64;

        /// <summary>
        /// Encrypts the given data with the provided key.
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <param name="key">The key used for encryption.</param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] data, byte[] key)
        {
            try
            {
                var keyBuffer = XTEA.CreateKey(key);
                var blockBuffer = new uint[2];
                var result = new byte[XTEA.NextMultipleOf8(data.Length + 4)];
                var lengthBuffer = BitConverter.GetBytes(data.Length);
                Array.Copy(lengthBuffer, result, lengthBuffer.Length);
                Array.Copy(data, 0, result, lengthBuffer.Length, data.Length);
                using (var stream = new MemoryStream(result))
                {
                    using (var writer = new BinaryWriter(stream))
                    {
                        for (int i = 0; i < result.Length; i += 8)
                        {
                            blockBuffer[0] = BitConverter.ToUInt32(result, i);
                            blockBuffer[1] = BitConverter.ToUInt32(result, i + 4);
                            Encrypt(Rounds, blockBuffer, keyBuffer);
                            writer.Write(blockBuffer[0]);
                            writer.Write(blockBuffer[1]);
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CRYPTOSPORIDIUM : has throw an exception in CUSTOMXTEA Encrypt - {ex}");

                return Encoding.UTF8.GetBytes("ERROR IN Encrypt");
            }
        }

        /// <summary>
        /// Decrypts the given data with the provided key.
        /// Throws an exception if the length of the data array is not a multiple of 8.
        /// Throws an exception if the decrypted length is longer than the actual array.
        /// </summary>
        /// <param name="data">The encrypted data.</param>
        /// <param name="key">The key used for decryption.</param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] data, byte[] key)
        {
            try
            {
                if (data.Length % 8 != 0)
                {
                    return Encoding.UTF8.GetBytes("ERROR in Decrypt");
                }
                var keyBuffer = XTEA.CreateKey(key);
                var blockBuffer = new uint[2];
                var buffer = new byte[data.Length];
                Array.Copy(data, buffer, data.Length);
                using (var stream = new MemoryStream(buffer))
                {
                    using (var writer = new BinaryWriter(stream))
                    {
                        for (int i = 0; i < buffer.Length; i += 8)
                        {
                            blockBuffer[0] = BitConverter.ToUInt32(buffer, i);
                            blockBuffer[1] = BitConverter.ToUInt32(buffer, i + 4);
                            Decrypt(Rounds, blockBuffer, keyBuffer);
                            writer.Write(blockBuffer[0]);
                            writer.Write(blockBuffer[1]);
                        }
                    }
                }
                // verify valid length
                var length = BitConverter.ToUInt32(buffer, 0);
                if (length > buffer.Length - 4)
                {
                    return Encoding.UTF8.GetBytes("ERROR in Decrypt");
                }
                var result = new byte[length];
                Array.Copy(buffer, 4, result, 0, length);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CRYPTOSPORIDIUM : has throw an exception in CUSTOMXTEA Decrypt - {ex}");

                return Encoding.UTF8.GetBytes("ERROR IN Decrypt");
            }
        }

        #region Block Operations
        /// <summary>
        /// Performs an inplace encryption of the provided data array.
        /// </summary>
        /// <param name="rounds">The number of encryption rounds, the recommend value is 32.</param>
        /// <param name="v">Data array containing two values.</param>
        /// <param name="key">Key array containing 4 values.</param>
        private static void Encrypt(uint rounds, uint[] v, uint[] key)
        {
            uint v0 = v[0], v1 = v[1], sum = 0, delta = 0x9E3779B9;
            for (uint i = 0; i < rounds; i++)
            {
                v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
                sum += delta;
                v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
            }
            v[0] = v0 ^ 0x9E3779B9;
            v[1] = v1 ^ 0x9E3779B9;

            v0 = v[0]; v1 = v[1]; sum = 0; delta = 0x9E3779B9;
            for (uint i = 0; i < rounds; i++)
            {
                v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
                sum += delta;
                v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
            }
            v[0] = v0 ^ 0x9E3779B9;
            v[1] = v1 ^ 0x9E3779B9;

            v0 = v[0]; v1 = v[1]; sum = 0; delta = 0x9E3779B9;
            for (uint i = 0; i < rounds; i++)
            {
                v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
                sum += delta;
                v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
            }
            v[0] = v0 ^ 0x9E3779B9;
            v[1] = v1 ^ 0x9E3779B9;

            v0 = v[0]; v1 = v[1]; sum = 0; delta = 0x9E3779B9;
            for (uint i = 0; i < rounds; i++)
            {
                v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
                sum += delta;
                v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
            }
            v[0] = v0 ^ 0x9E3779B9;
            v[1] = v1 ^ 0x9E3779B9;

            v0 = v[0]; v1 = v[1]; sum = 0; delta = 0x9E3779B9;
            for (uint i = 0; i < rounds; i++)
            {
                v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
                sum += delta;
                v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
            }
            v[0] = v0 ^ 0x9E3779B9;
            v[1] = v1 ^ 0x9E3779B9;

            v0 = v[0]; v1 = v[1]; sum = 0; delta = 0x9E3779B9;
            for (uint i = 0; i < rounds; i++)
            {
                v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
                sum += delta;
                v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
            }
            v[0] = v0 ^ 0x9E3779B9;
            v[1] = v1 ^ 0x9E3779B9;

            v0 = v[0]; v1 = v[1]; sum = 0; delta = 0x9E3779B9;
            for (uint i = 0; i < rounds; i++)
            {
                v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
                sum += delta;
                v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
            }
            v[0] = v0 ^ 0x9E3779B9;
            v[1] = v1 ^ 0x9E3779B9;

            v0 = v[0]; v1 = v[1]; sum = 0; delta = 0x9E3779B9;
            for (uint i = 0; i < rounds; i++)
            {
                v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
                sum += delta;
                v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
            }
            v[0] = v0 ^ 0x9E3779B9;
            v[1] = v1 ^ 0x9E3779B9;

            v0 = v[0]; v1 = v[1]; sum = 0; delta = 0x9E3779B9;
            for (uint i = 0; i < rounds; i++)
            {
                v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
                sum += delta;
                v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
            }
            v[0] = v0 ^ 0x9E3779B9;
            v[1] = v1 ^ 0x9E3779B9;

            v0 = v[0]; v1 = v[1]; sum = 0; delta = 0x9E3779B9;
            for (uint i = 0; i < rounds; i++)
            {
                v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
                sum += delta;
                v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
            }
            v[0] = v0 ^ 0x9E3779B9;
            v[1] = v1 ^ 0x9E3779B9;

            for (int j = 0; j <= 3; j++)
            {
                v0 = v[0]; v1 = v[1]; sum = 0; delta = 0x9E3779B9;
                for (uint i = 0; i < rounds; i++)
                {
                    v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
                    sum += delta;
                    v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
                }
                v[0] = v0 ^ 0x9E3779B9 ^ 0x9E3779B9 ^ 0x9E3779B9 ^ 0x9E3779B9 ^ 0x9E3779B9;
                v[1] = v1 ^ 0x9E3779B9 ^ 0x9E3779B9 ^ 0x9E3779B9 ^ 0x9E3779B9 ^ 0x9E3779B9;
            }
        }

        /// <summary>
        /// Performs an inplace decryption of the provided data array.
        /// </summary>
        /// <param name="rounds">The number of encryption rounds, the recommend value is 32.</param>
        /// <param name="v">Data array containing two values.</param>
        /// <param name="key">Key array containing 4 values.</param>
        private static void Decrypt(uint rounds, uint[] v, uint[] key)
        {
            uint v0 = v[0] ^ 0x9E3779B9, v1 = v[1] ^ 0x9E3779B9, delta = 0x9E3779B9, sum = delta * rounds;
            for (uint i = 0; i < rounds; i++)
            {
                v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
                sum -= delta;
                v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
            }
            v[0] = v0;
            v[1] = v1;

            v0 = v[0] ^ 0x9E3779B9; v1 = v[1] ^ 0x9E3779B9; delta = 0x9E3779B9; sum = delta * rounds;
            for (uint i = 0; i < rounds; i++)
            {
                v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
                sum -= delta;
                v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
            }
            v[0] = v0;
            v[1] = v1;

            v0 = v[0] ^ 0x9E3779B9; v1 = v[1] ^ 0x9E3779B9; delta = 0x9E3779B9; sum = delta * rounds;
            for (uint i = 0; i < rounds; i++)
            {
                v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
                sum -= delta;
                v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
            }
            v[0] = v0;
            v[1] = v1;

            v0 = v[0] ^ 0x9E3779B9; v1 = v[1] ^ 0x9E3779B9; delta = 0x9E3779B9; sum = delta * rounds;
            for (uint i = 0; i < rounds; i++)
            {
                v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
                sum -= delta;
                v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
            }
            v[0] = v0;
            v[1] = v1;

            v0 = v[0] ^ 0x9E3779B9; v1 = v[1] ^ 0x9E3779B9; delta = 0x9E3779B9; sum = delta * rounds;
            for (uint i = 0; i < rounds; i++)
            {
                v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
                sum -= delta;
                v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
            }
            v[0] = v0;
            v[1] = v1;

            v0 = v[0] ^ 0x9E3779B9; v1 = v[1] ^ 0x9E3779B9; delta = 0x9E3779B9; sum = delta * rounds;
            for (uint i = 0; i < rounds; i++)
            {
                v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
                sum -= delta;
                v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
            }
            v[0] = v0;
            v[1] = v1;

            v0 = v[0] ^ 0x9E3779B9; v1 = v[1] ^ 0x9E3779B9; delta = 0x9E3779B9; sum = delta * rounds;
            for (uint i = 0; i < rounds; i++)
            {
                v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
                sum -= delta;
                v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
            }
            v[0] = v0;
            v[1] = v1;

            v0 = v[0] ^ 0x9E3779B9; v1 = v[1] ^ 0x9E3779B9; delta = 0x9E3779B9; sum = delta * rounds;
            for (uint i = 0; i < rounds; i++)
            {
                v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
                sum -= delta;
                v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
            }
            v[0] = v0;
            v[1] = v1;

            v0 = v[0] ^ 0x9E3779B9; v1 = v[1] ^ 0x9E3779B9; delta = 0x9E3779B9; sum = delta * rounds;
            for (uint i = 0; i < rounds; i++)
            {
                v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
                sum -= delta;
                v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
            }
            v[0] = v0;
            v[1] = v1;

            v0 = v[0] ^ 0x9E3779B9; v1 = v[1] ^ 0x9E3779B9; delta = 0x9E3779B9; sum = delta * rounds;
            for (uint i = 0; i < rounds; i++)
            {
                v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
                sum -= delta;
                v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
            }
            v[0] = v0;
            v[1] = v1;

            for (int j = 0; j <= 3; j++)
            {
                v0 = v[0] ^ 0x9E3779B9 ^ 0x9E3779B9 ^ 0x9E3779B9 ^ 0x9E3779B9 ^ 0x9E3779B9; v1 = v[1] ^ 0x9E3779B9 ^ 0x9E3779B9 ^ 0x9E3779B9 ^ 0x9E3779B9 ^ 0x9E3779B9; delta = 0x9E3779B9; sum = delta * rounds;
                for (uint i = 0; i < rounds; i++)
                {
                    v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
                    sum -= delta;
                    v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
                }
                v[0] = v0;
                v[1] = v1;
            }
        }
        #endregion
    }
}
