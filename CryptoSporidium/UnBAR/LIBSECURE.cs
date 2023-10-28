using CustomLogger;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.Text;

namespace CryptoSporidium.UnBAR
{
    internal class LIBSECURE
    {
        public byte[]? ProcessXTEABlocks(byte[] inputArray, byte[] Key, byte[] IV)
        {
            int inputLength = inputArray.Length;
            int inputIndex = 0;
            byte[]? output = null;
            ToolsImpl? toolsimpl = new();

            while (inputIndex < inputLength)
            {
                int blockSize = Math.Min(8, inputLength - inputIndex);
                byte[]? block = new byte[blockSize];
                Buffer.BlockCopy(inputArray, inputIndex, block, 0, blockSize);

                block = InitiateXTEACTRBuffer(block, Key, IV, blockSize);

                toolsimpl.IncrementIVBytes(IV, 1);

                if (output == null)
                    output = block;
                else
                {
                    Utils? utils = new();
                    output = utils.Combinebytearay(output, block);
                    utils = null;
                }

                inputIndex += blockSize;
            }

            toolsimpl = null;

            return output;
        }

        public async Task<byte[]>? ProcessXTEABlocksAsync(byte[] inputArray, byte[] Key, byte[] IV)
        {
            int inputLength = inputArray.Length;
            int inputIndex = 0;
            int blockSize = 8;
            int outputIndex = 0;
            int executionNumber = 0;
            byte[]? output = new byte[inputLength];
            SemaphoreSlim semaphore = new(1);
            ToolsImpl? toolsimpl = new();

            while (inputIndex < inputLength)
            {
                blockSize = Math.Min(8, inputLength - inputIndex);
                byte[] block = new byte[blockSize];
                Buffer.BlockCopy(inputArray, inputIndex, block, 0, blockSize);
                int currentExecutionNumber = executionNumber;

                var tcs = new TaskCompletionSource<byte[]>();

                await Task.Run(() =>
                {
                    tcs.SetResult(InitiateXTEACTRBuffer(block, Key, IV, blockSize));
                });

                toolsimpl.IncrementIVBytes(IV, 1);

                await semaphore.WaitAsync();

                byte[] taskResult = await tcs.Task;
                Buffer.BlockCopy(taskResult, 0, output, outputIndex, blockSize);
                outputIndex += blockSize;
                semaphore.Release();

                inputIndex += blockSize;
                executionNumber++;
            }

            semaphore.Dispose();
            toolsimpl = null;

            return output;
        }

        public byte[]? InitiateXTEACTRBuffer(byte[] FileBytes, byte[] KeyBytes, byte[] m_iv, int blocksize)
        {
            if (KeyBytes != null && KeyBytes.Length >= 16 && m_iv != null && m_iv.Length == 8 && FileBytes != null)
            {
                Utils? utils = new();
                byte[]? returnbytes = null;
                // Create the cipher
                IBufferedCipher? cipher = null;

                if (blocksize != 8)
                    cipher = CipherUtilities.GetCipher("LIBSECUREXTEA/ENDIANCTR/ZEROBYTEPADDING");
                else
                    cipher = CipherUtilities.GetCipher("LIBSECUREXTEA/ENDIANCTR/NOPADDING");

                cipher.Init(true, new ParametersWithIV(new KeyParameter(KeyBytes), m_iv)); // Bouncy Castle not like padding in decrypt mode with custom data.

                // Encrypt the plaintext
                byte[] ciphertextBytes = new byte[cipher.GetOutputSize(FileBytes.Length)];
                int ciphertextLength = cipher.ProcessBytes(FileBytes, 0, FileBytes.Length, ciphertextBytes, 0);
                cipher.DoFinal(ciphertextBytes, ciphertextLength);

                cipher = null;

                if (BitConverter.IsLittleEndian) // KeyBytes endian check directly in libsecure.
                {
                    returnbytes = utils.GetRequiredBlocks(Org.BouncyCastle.util.EndianTools.ReverseEndiannessInChunks(ciphertextBytes, 4), blocksize);
                    utils = null;
                    return returnbytes;
                }
                else
                {
                    returnbytes = utils.GetRequiredBlocks(ciphertextBytes, blocksize);
                    return returnbytes;
                }
            }
            else
                LoggerAccessor.LogError("[LIBSECURE] - InitiateXTEACTRBuffer - Invalid FileBytes, KeyByes or IV!");

            return null;
        }

        public string MemXOR(string IV, string block, int blocksize)
        {
            string? returnstring = null;
            StringBuilder? CryptoBytes = new();
            Utils? utils = new();

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
                        CryptoBytes.Append(utils.ByteArrayToHexString(utils.HexStringToByteArray(output)));
                    }
                    catch (Exception ex)
                    {
                        LoggerAccessor.LogError($"[LIBSECURE] - Error In MemXOR: {ex}");
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
                        CryptoBytes.Append(utils.ByteArrayToHexString(utils.HexStringToByteArray(output)));
                    }
                    catch (Exception ex)
                    {
                        LoggerAccessor.LogError($"[LIBSECURE] - Error In MemXOR: {ex}");
                    }
                }
            }

            returnstring = CryptoBytes.ToString();

            CryptoBytes = null;
            utils = null;

            return returnstring;
        }
    }
}
