
using System;
using System.Security.Cryptography;

namespace WebAPIService.CDS
{
    public class BruteforceProcess
    {
        private byte[]? DecryptedFileBytes = null;
        private byte[]? EncryptedFileBytes = null;

        public BruteforceProcess(byte[] EncryptedFileBytes)
        {
            this.EncryptedFileBytes = EncryptedFileBytes;
        }

        public byte[]? StartBruteForce(int mode = 0)
        {
            if (EncryptedFileBytes != null)
            {
                DateTime timeStarted = DateTime.Now;
                CustomLogger.LoggerAccessor.LogWarn("[CDS] - BruteforceProcess - BruteForce started at: - {0}", timeStarted.ToString());

                byte[] TempBuffer = new byte[8];

                Buffer.BlockCopy(EncryptedFileBytes, 0, TempBuffer, 0, TempBuffer.Length);

                DecryptedFileBytes = CTRExploitProcess.ProcessExploit(TempBuffer, EncryptedFileBytes, mode);

                if (DecryptedFileBytes != null)
                {
                    byte[] SHA1Data;
                    using (SHA1 sha1 = SHA1.Create())
                    {
                        SHA1Data = sha1.ComputeHash(DecryptedFileBytes);
                    }
                    CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - Resolved SHA1: {0}", BitConverter.ToString(SHA1Data).Replace("-", string.Empty).ToUpper());
                }
                else
                    CustomLogger.LoggerAccessor.LogError("[CDS] - BruteforceProcess - Nothing matched! - Make sure input was correct. - {0}", DateTime.Now.ToString());
#if DEBUG
                CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - Time passed: {0}s", DateTime.Now.Subtract(timeStarted).TotalSeconds);
#endif
            }

            return DecryptedFileBytes;
        }
    }
}
