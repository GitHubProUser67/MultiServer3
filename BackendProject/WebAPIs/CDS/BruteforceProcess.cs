using System.Text;
using System.Xml;

namespace BackendProject.WebAPIs.CDS
{
    public class BruteforceProcess
    {
        #region Private variables

        private string ResultIV = string.Empty;
        private byte[] DecryptedFileBytes = Array.Empty<byte>();
        private byte[] EncryptedFileBytes = Array.Empty<byte>();

        public BruteforceProcess(byte[] EncryptedFileBytes)
        {
            this.EncryptedFileBytes = EncryptedFileBytes;
        }

        private bool isMatched = false;

        /* The length of the charactersToTest Array is stored in a
         * additional variable to increase performance  */
        private int charactersToTestLength = 0;
        private long computedKeys = 0;

        /* An array containing the characters which will be used to create the brute force keys,
         * if less characters are used (e.g. only upper case chars) the faster the password is matched.
           In case of the CDS, only part of the SHA1 is needed, and only charset used is upper case. */
        private char[] charactersToTest =
        {
            'A','B','C','D','E','F','1','2','3','4','5',
            '6','7','8','9','0'
        };

        // Create a List to store SHA1 entries
        private List<string> sha1Entries = new();

        #endregion

        public char[] GetCharArray(string input)
        {
            // Check if the input string is in the specified format.
            if (!string.IsNullOrEmpty(input) && input.Contains(',') && input.Split(',').All(s => s.Length == 1))
                // Convert each string element to a character and return the array.
                return input.Split(',').Select(s => s[0]).ToArray();

            // Invalid format, return original array.
            return charactersToTest;
        }

        public async Task<byte[]> StartBruteForce(string helperfolder, string charset)
        {
            DateTime timeStarted = DateTime.Now;
            CustomLogger.LoggerAccessor.LogWarn("[CDS] - BruteforceProcess - BruteForce started at: - {0}", timeStarted.ToString());

            byte[] TempBuffer = MiscUtils.CopyBytes(EncryptedFileBytes, 0, 8);

            if (TempBuffer != Array.Empty<byte>())
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
                                    if (ProcessedFileBytes.Length > 4 && (ProcessedFileBytes[0] == 0x3c && ProcessedFileBytes[1] == 0x78 && ProcessedFileBytes[2] == 0x6d && ProcessedFileBytes[3] == 0x6c
                                                            || ProcessedFileBytes[0] == 0x3c && ProcessedFileBytes[1] == 0x58 && ProcessedFileBytes[2] == 0x4d && ProcessedFileBytes[3] == 0x4c
                                                            || ProcessedFileBytes[0] == 0x3c && ProcessedFileBytes[1] == 0x3f && ProcessedFileBytes[2] == 0x78 && ProcessedFileBytes[3] == 0x6d
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

                                    if (DecryptedFileBytes != Array.Empty<byte>())
                                    {
                                        if (!isMatched)
                                        {
                                            isMatched = true;
                                            ResultIV = sha1;
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                if (isMatched)
                {
                    CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - IV matched! - {0}", DateTime.Now.ToString());
                    CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - Resolved IV: {0}", ResultIV);
                }
                else
                {
                    if (!string.IsNullOrEmpty(charset))
                        charactersToTest = GetCharArray(charset);

                    // The length of the array is stored permanently during runtime
                    charactersToTestLength = charactersToTest.Length;

                    await startBruteForce(16, TempBuffer); // SHA1 IV is always 16 characters.

                    if (ResultIV == "NOMATCH")
                    {
                        CustomLogger.LoggerAccessor.LogError("[CDS] - BruteforceProcess - Nothing matched! - Make sure the input was correct. - {0}", DateTime.Now.ToString());
                        DecryptedFileBytes = Encoding.UTF8.GetBytes(ResultIV);
                    }
                    else
                    {
                        CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - IV matched! - {0}", DateTime.Now.ToString());
                        CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - Resolved IV: {0}", ResultIV);
                    }
                }
#if DEBUG
                CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - Time passed: {0}s", DateTime.Now.Subtract(timeStarted).TotalSeconds);
                CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - Computed IVs: {0}", computedKeys);
#endif
            }
            else
            {
                CustomLogger.LoggerAccessor.LogError("[CDS] - BruteforceProcess - The input data is not a blowfish compliant file. - {0}", DateTime.Now.ToString());
                DecryptedFileBytes = Encoding.UTF8.GetBytes("INVALID");
            }

            return DecryptedFileBytes;
        }

        #region Private methods

        /// <summary>
        /// Starts the recursive method which will create the keys via brute force
        /// </summary>
        /// <param name="keyLength">The length of the key</param>
        private async Task startBruteForce(int keyLength, byte[] TempBuffer)
        {
            // The index of the last character will be stored for slight perfomance improvement
            await createNewKey(0, createCharArray(keyLength, charactersToTest[0]), keyLength, keyLength - 1, TempBuffer);
        }

        /// <summary>
        /// Creates a new char array of a specific length filled with the defaultChar
        /// </summary>
        /// <param name="length">The length of the array</param>
        /// <param name="defaultChar">The char with whom the array will be filled</param>
        /// <returns></returns>
        private char[] createCharArray(int length, char defaultChar)
        {
            return (from c in new char[length] select defaultChar).ToArray();
        }

        /// <summary>
        /// This is the main workhorse, it creates new keys and compares them to the password until the password
        /// is matched or all keys of the current key length have been checked
        /// </summary>
        /// <param name="currentCharPosition">The position of the char which is replaced by new characters currently</param>
        /// <param name="keyChars">The current key represented as char array</param>
        /// <param name="keyLength">The length of the key</param>
        /// <param name="indexOfLastChar">The index of the last character of the key</param>
        private async Task createNewKey(int currentCharPosition, char[] keyChars, int keyLength, int indexOfLastChar, byte[] TempBuffer)
        {
            if (!isMatched) // This one will allow to stop recursive tasks if one of these tasks found a valid result.
            {
                // We are looping trough the full length of our charactersToTest array
                for (int i = 0; i < charactersToTestLength; i++)
                {
                    /* The character at the currentCharPosition will be replaced by a
                     * new character from the charactersToTest array => a new key combination will be created */
                    keyChars[currentCharPosition] = charactersToTest[i];

                    // The method calls itself recursively until all positions of the key char array have been replaced
                    if (currentCharPosition < indexOfLastChar)
                        await createNewKey(currentCharPosition + 1, keyChars, keyLength, indexOfLastChar, TempBuffer);
                    else
                    {
                        // A new key has been created, remove this counter to improve performance
                        computedKeys++;

                        /* The char array will be converted to a string and compared to the password. If the password
                         * is matched the loop breaks and the password is stored as result. */
                        string keyset = new(keyChars);
#if DEBUG
                        CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - Current IV Value - {0}", keyset);
#endif
                        byte[]? ProcessedFileBytes = CDSProcess.CDSEncrypt_Decrypt(TempBuffer, keyset);

                        if (ProcessedFileBytes != null)
                        {
                            if (ProcessedFileBytes.Length > 4 && (ProcessedFileBytes[0] == 0x3c && ProcessedFileBytes[1] == 0x78 && ProcessedFileBytes[2] == 0x6d && ProcessedFileBytes[3] == 0x6c
                                                    || ProcessedFileBytes[0] == 0x3c && ProcessedFileBytes[1] == 0x58 && ProcessedFileBytes[2] == 0x4d && ProcessedFileBytes[3] == 0x4c
                                                    || ProcessedFileBytes[0] == 0x3c && ProcessedFileBytes[1] == 0x3f && ProcessedFileBytes[2] == 0x78 && ProcessedFileBytes[3] == 0x6d
                                                    || ProcessedFileBytes[0] == 0x3c && ProcessedFileBytes[1] == 0x53 && ProcessedFileBytes[2] == 0x43 && ProcessedFileBytes[3] == 0x45))
                            {
                                CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - File Type: XML Dectected! - {0}", DateTime.Now.ToString());
                                DecryptedFileBytes = CDSProcess.CDSEncrypt_Decrypt(EncryptedFileBytes, keyset) ?? Array.Empty<byte>();
                            }
                            else if (ProcessedFileBytes.Length > 4 && ProcessedFileBytes[0] == 0x73 && ProcessedFileBytes[1] == 0x65 && ProcessedFileBytes[2] == 0x67 && ProcessedFileBytes[3] == 0x73)
                            {
                                CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - File Type: HCDB Dectected! - {0}", DateTime.Now.ToString());
                                DecryptedFileBytes = CDSProcess.CDSEncrypt_Decrypt(EncryptedFileBytes, keyset) ?? Array.Empty<byte>();
                            }
                            else if (ProcessedFileBytes.Length > 4 && ProcessedFileBytes[0] == 0xAD && ProcessedFileBytes[1] == 0xEF && ProcessedFileBytes[2] == 0x17 && ProcessedFileBytes[3] == 0xE1)
                            {
                                CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - File Type: BAR BIG ENDIAN Dectected! - {0}", DateTime.Now.ToString());
                                DecryptedFileBytes = CDSProcess.CDSEncrypt_Decrypt(EncryptedFileBytes, keyset) ?? Array.Empty<byte>();
                            }
                            else if (ProcessedFileBytes.Length > 4 && ProcessedFileBytes[0] == 0xE1 && ProcessedFileBytes[1] == 0x17 && ProcessedFileBytes[2] == 0xEF && ProcessedFileBytes[3] == 0xAD)
                            {
                                CustomLogger.LoggerAccessor.LogInfo("[CDS] - BruteforceProcess - File Type: BAR LITTLE ENDIAN Dectected! - {0}", DateTime.Now.ToString());
                                DecryptedFileBytes = CDSProcess.CDSEncrypt_Decrypt(EncryptedFileBytes, keyset) ?? Array.Empty<byte>();
                            }

                            if (DecryptedFileBytes != Array.Empty<byte>())
                            {
                                if (!isMatched)
                                {
                                    isMatched = true;
                                    ResultIV = keyset;
                                }
                                return;
                            }
                        }

                        if (keyset == new string(charactersToTest[^1], 16)) // We tried all key combinations, so the bruteforce failed.
                        {
                            if (!isMatched)
                            {
                                isMatched = true;
                                ResultIV = "NOMATCH";
                            }
                            return;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
