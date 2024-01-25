using BackendProject.MiscUtils;
using System.Security.Cryptography;
using System.Xml;

namespace BackendProject.WebAPIs.CDS
{
    public class BruteforceProcess
    {
        private byte[]? DecryptedFileBytes = null;
        private byte[]? EncryptedFileBytes = null;

        // Create a List to store SHA1 entries
        private List<string> sha1Entries = new();

        public BruteforceProcess(byte[] EncryptedFileBytes)
        {
            this.EncryptedFileBytes = EncryptedFileBytes;
        }

        public byte[]? StartBruteForce(string helperfolder, int mode = 0)
        {
            if (EncryptedFileBytes != null)
            {
                DateTime timeStarted = DateTime.Now;
                CustomLogger.LoggerAccessor.LogWarn("[CDS] - BruteforceProcess - BruteForce started at: - {0}", timeStarted.ToString());

                byte[]? TempBuffer = VariousUtils.CopyBytes(EncryptedFileBytes, 0, 8);

                if (TempBuffer != null)
                {
                    if (File.Exists(helperfolder + "/CDSBruteforceRecord.xml"))
                    {
                        CustomLogger.LoggerAccessor.LogWarn("[CDS] - BruteforceProcess - Record Checking In-Progress...");

                        // Create a new XmlDocument and load the XML string
                        XmlDocument xmlDoc = new();
                        xmlDoc.LoadXml(File.ReadAllText(helperfolder + "/CDSBruteforceRecord.xml"));

                        // Select all SHA1 elements using XPath
                        XmlNodeList? sha1Nodes = xmlDoc.SelectNodes("//SHA1");

                        if (sha1Nodes != null)
                        {
                            // Iterate through each SHA1 element and add its value to the list
                            foreach (XmlNode sha1Node in sha1Nodes)
                            {
                                sha1Entries.Add(sha1Node.InnerText);
                            }

                            foreach (string sha1 in sha1Entries)
                            {
                                if (!string.IsNullOrEmpty(sha1) && sha1.Length >= 16)
                                {
                                    byte[]? ProcessedFileBytes = CDSProcess.CDSEncrypt_Decrypt(TempBuffer, sha1[..16]);

                                    if (ProcessedFileBytes != null)
                                    {
                                        if (ProcessedFileBytes.Length >= 8 && (ProcessedFileBytes[0] == 0x3c && ProcessedFileBytes[1] == 0x78 && ProcessedFileBytes[2] == 0x6d && ProcessedFileBytes[3] == 0x6c
                                                                || ProcessedFileBytes[0] == 0x3c && ProcessedFileBytes[1] == 0x58 && ProcessedFileBytes[2] == 0x4d && ProcessedFileBytes[3] == 0x4c
                                                                || ProcessedFileBytes[0] == 0xEF && ProcessedFileBytes[1] == 0xBB && ProcessedFileBytes[2] == 0xBF && ProcessedFileBytes[3] == 0x3C && ProcessedFileBytes[4] == 0x3F && ProcessedFileBytes[5] == 0x78 && ProcessedFileBytes[6] == 0x6D && ProcessedFileBytes[7] == 0x6C
                                                                || ProcessedFileBytes[0] == 0x3C && ProcessedFileBytes[1] == 0x3F && ProcessedFileBytes[2] == 0x78 && ProcessedFileBytes[3] == 0x6D && ProcessedFileBytes[4] == 0x6C && ProcessedFileBytes[5] == 0x20 && ProcessedFileBytes[6] == 0x76 && ProcessedFileBytes[7] == 0x65
                                                                || ProcessedFileBytes[0] == 0x3c && ProcessedFileBytes[1] == 0x53 && ProcessedFileBytes[2] == 0x43 && ProcessedFileBytes[3] == 0x45))
                                        {
                                            CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - File Type: XML Dectected! - {0}", DateTime.Now.ToString());
                                            DecryptedFileBytes = CDSProcess.CDSEncrypt_Decrypt(EncryptedFileBytes, sha1[..16]) ?? Array.Empty<byte>();
                                        }
                                        else if (ProcessedFileBytes.Length > 4 && ProcessedFileBytes[0] == 0x73 && ProcessedFileBytes[1] == 0x65 && ProcessedFileBytes[2] == 0x67 && ProcessedFileBytes[3] == 0x73)
                                        {
                                            CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - File Type: HCDB Dectected! - {0}", DateTime.Now.ToString());
                                            DecryptedFileBytes = CDSProcess.CDSEncrypt_Decrypt(EncryptedFileBytes, sha1[..16]) ?? Array.Empty<byte>();
                                        }
                                        else if (ProcessedFileBytes.Length > 4 && ProcessedFileBytes[0] == 0xAD && ProcessedFileBytes[1] == 0xEF && ProcessedFileBytes[2] == 0x17 && ProcessedFileBytes[3] == 0xE1)
                                        {
                                            CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - File Type: BAR BIG ENDIAN Dectected! - {0}", DateTime.Now.ToString());
                                            DecryptedFileBytes = CDSProcess.CDSEncrypt_Decrypt(EncryptedFileBytes, sha1[..16]) ?? Array.Empty<byte>();
                                        }
                                        else if (ProcessedFileBytes.Length > 4 && ProcessedFileBytes[0] == 0xE1 && ProcessedFileBytes[1] == 0x17 && ProcessedFileBytes[2] == 0xEF && ProcessedFileBytes[3] == 0xAD)
                                        {
                                            CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - File Type: BAR LITTLE ENDIAN Dectected! - {0}", DateTime.Now.ToString());
                                            DecryptedFileBytes = CDSProcess.CDSEncrypt_Decrypt(EncryptedFileBytes, sha1[..16]) ?? Array.Empty<byte>();
                                        }

                                        if (DecryptedFileBytes != null)
                                        {
                                            CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - IV matched! - {0}", DateTime.Now.ToString());
                                            CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - Resolved IV: {0}", sha1);
                                            
                                            return DecryptedFileBytes;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    DecryptedFileBytes = CTRExploitProcess.ProcessExploit(TempBuffer, EncryptedFileBytes, mode);

                    if (DecryptedFileBytes != null)
                    {
                        using (SHA1 sha1 = SHA1.Create())
                        {
                            CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - Resolved SHA1: {0}", BitConverter.ToString(sha1.ComputeHash(DecryptedFileBytes)).Replace("-", "").ToUpper());
                            sha1.Clear();
                        }
                    }
                    else
                        CustomLogger.LoggerAccessor.LogError("[CDS] - BruteforceProcess - Nothing matched! - Make sure input was correct. - {0}", DateTime.Now.ToString());
#if DEBUG
                    CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - Time passed: {0}s", DateTime.Now.Subtract(timeStarted).TotalSeconds);
#endif
                }
                else
                    CustomLogger.LoggerAccessor.LogError("[CDS] - BruteforceProcess - The input data failed to copy! Make sure input was correct. - {0}", DateTime.Now.ToString());
            }

            return DecryptedFileBytes;
        }
    }
}
