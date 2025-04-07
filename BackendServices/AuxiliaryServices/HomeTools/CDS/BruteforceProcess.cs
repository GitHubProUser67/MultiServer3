using System;
using HashLib;

namespace HomeTools.CDS
{
    public class BruteforceProcess
    {
        private byte[] DecryptedFileBytes = null;
        private byte[] EncryptedFileBytes = null;

        public BruteforceProcess(byte[] EncryptedFileBytes)
        {
            this.EncryptedFileBytes = EncryptedFileBytes;
        }

        public byte[] StartBruteForce(ushort cdnMode, int mode = 0)
        {
            if (EncryptedFileBytes != null)
            {
                DateTime timeStarted = DateTime.Now;
                CustomLogger.LoggerAccessor.LogWarn("[CDS] - BruteforceProcess - BruteForce started at: - {0}", timeStarted.ToString());

                byte[] TempBuffer = new byte[8];

                Buffer.BlockCopy(EncryptedFileBytes, 0, TempBuffer, 0, TempBuffer.Length);

                DecryptedFileBytes = CTRExploitProcess.ProcessExploit(TempBuffer, EncryptedFileBytes, mode, cdnMode);

                if (DecryptedFileBytes != null)
                    CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - Resolved SHA1: {0}", NetHasher.ComputeSHA1String(DecryptedFileBytes));
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
