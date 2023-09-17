using MultiServer.CryptoSporidium.BAR;

namespace MultiServer.CryptoSporidium.UnBAR
{
    internal class RunUnBAR
    {
        public static void Run(string inputfile, string outputpath, bool edat)
        {
            if (edat)
                RunDecrypt(inputfile, outputpath);
            else
                RunExtract(inputfile, outputpath);
        }

        public static void RunEncrypt(string filePath, string sdatfilePath, string sdatnpdcopyfile)
        {
            new EDAT().encryptFile(filePath, sdatfilePath, sdatnpdcopyfile);
        }

        private static void RunDecrypt(string filePath, string outDir)
        {
            new EDAT().decryptFile(filePath, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath) + ".dat"));

            RunExtract(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath) + ".dat"), outDir);
        }

        private static void RunExtract(string filePath, string outDir)
        {
            ServerConfiguration.LogInfo("Loading BAR/dat: {0}", filePath);

            byte[] RawBarData = null;

            if (File.Exists(filePath))
            {
                try
                {
                    RawBarData = File.ReadAllBytes(filePath);

                    byte[] data = Misc.ReadBinaryFile(filePath, 0x0C, 4); // Read 4 bytes from offset 0x0C to 0x0F

                    string formattedData = BitConverter.ToString(data).Replace("-", ""); // Convert byte array to hex string

                    Directory.CreateDirectory(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)));

                    File.WriteAllText(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)) + "/timestamp.txt", formattedData);
                }
                catch (Exception ex)
                {
                    ServerConfiguration.LogError($"[RunUnBAR] - Timestamp creation failed! with error - {ex}");
                }
            }

            try
            {
                BARArchive archive = new(filePath, outDir);
                archive.Load();
                foreach (TOCEntry tableOfContent in archive.TableOfContents)
                {
                    MemoryStream memoryStream = new MemoryStream(tableOfContent.GetData(archive.GetHeader().Flags));
                    try
                    {
                        string registeredExtension = FileTypeAnalyser.Instance.GetRegisteredExtension(FileTypeAnalyser.Instance.Analyse(memoryStream));
                        ExtractToFile(RawBarData, archive, tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)), registeredExtension);
                    }
                    catch (Exception ex)
                    {
                        ServerConfiguration.LogWarn(ex.ToString());
                        string fileType = ".unknown";
                        ExtractToFile(RawBarData, archive, tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)), fileType);
                    }
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[RunUnBAR] - RunExtract Errored out - {ex}");
            }
        }

        public static void ExtractToFile(byte[] RawBarData, BARArchive archive, HashedFileName FileName, string outDir, string fileType)
        {
            TOCEntry tableOfContent = archive.TableOfContents[FileName];
            string empty = string.Empty;
            string path = string.Empty;
            if (tableOfContent.Path == null || tableOfContent.Path == string.Empty)
                path = string.Format("{0}{1}{2:X8}{3}", outDir, Path.DirectorySeparatorChar, FileName.Value, fileType);
            else
            {
                string str = tableOfContent.Path.Replace('/', Path.DirectorySeparatorChar);
                path = string.Format("{0}{1}{2}", outDir, Path.DirectorySeparatorChar, str);
            }
            string outdirectory = Path.GetDirectoryName(path);
            Directory.CreateDirectory(outdirectory);
            FileStream fileStream = File.Open(path, (FileMode)2);
            byte[] data = tableOfContent.GetData(archive.GetHeader().Flags);
            fileStream.Write(data, 0, data.Length);
            fileStream.Close();
            ServerConfiguration.LogInfo("Extracted file {0}", new object[1]
            {
                 Path.GetFileName(path)
            });

            if (data[0] == 0x00 && data[1] == 0x00 && data[2] == 0x00 && data[3] == 0x01 && tableOfContent.Compression == CompressionMethod.Encrypted)
            {
                if (File.Exists(outDir + "/timestamp.txt"))
                {
                    int dataStart = Misc.FindDataPosInBinary(RawBarData, data);

                    if (dataStart != -1)
                    {
                        uint compressedSize = tableOfContent.CompressedSize;
                        uint fileSize = tableOfContent.Size;
                        string content = File.ReadAllText(outDir + "/timestamp.txt");
                        int userData = BitConverter.ToInt32(Misc.HexStringToByteArray(content));

                        ServerConfiguration.LogInfo("[RunUnBAR] - Encrypted Content Detected!, running signature decryption.");
                        ServerConfiguration.LogInfo($"CompressedSize - {compressedSize}");
                        ServerConfiguration.LogInfo($"Size - {fileSize}");
                        ServerConfiguration.LogInfo($"dataStart - 0x{dataStart:X}");
                        ServerConfiguration.LogInfo($"UserData - 0x{userData:X}");

                        byte[] byteSignatureIV = BitConverter.GetBytes(AFSMISC.BuildSignatureIv((int)fileSize, (int)compressedSize, dataStart, userData));

                        // If you want to ensure little-endian byte order explicitly, you can reverse the array
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(byteSignatureIV);

                        ServerConfiguration.LogInfo($"SignatureIV - {Misc.ByteArrayToHexString(byteSignatureIV)}");

                        data = AFSMISC.RemovePaddingPrefix(data);

                        byte[] EncryptedHeaderSHA1 = new byte[24];

                        // Copy the first 24 bytes from the source array to the destination array
                        Array.Copy(data, 0, EncryptedHeaderSHA1, 0, EncryptedHeaderSHA1.Length);

                        byte[] SHA1DATA = AFSBLOWFISH.EncryptionProxyInit(EncryptedHeaderSHA1, byteSignatureIV);

                        if (SHA1DATA != null)
                        {
                            string verificationsha1 = Misc.ByteArrayToHexString(SHA1DATA);

                            // Create a new byte array to store the remaining content
                            byte[] newFileBytes = new byte[data.Length - 24];

                            // Copy the content after the first 24 bytes to the new array
                            Array.Copy(data, 24, newFileBytes, 0, newFileBytes.Length);

                            string sha1 = AFSMISC.ValidateSha1(newFileBytes);

                            if (sha1 == verificationsha1.Substring(0, verificationsha1.Length - 8))
                            {
                                ServerConfiguration.LogInfo("[RunUnBAR] - Lua file has not been tempered with.");

                                // Todo, something related to encryption is happening after.
                                // It seems it want the default key and a default context size then decrypt file bytes with this.

                                //byte[] buffer = CompressionFactory.Decompress(newFileBytes, CompressionMethod.EdgeZLib, ArchiveFlags.Bar_Flag_ZTOC); // Not good!
                            }
                            else
                                ServerConfiguration.LogWarn($"[RunUnBAR] - Lua file (SHA1 - {sha1}) has been tempered with! (Reference SHA1 - {verificationsha1.Substring(0, verificationsha1.Length - 8)}), Aborting decryption.");
                        }
                    }
                    else
                        ServerConfiguration.LogError("[RunUnBAR] - Encrypted data not found in BAR or false positive! Decryption has failed.");
                }
                else
                    ServerConfiguration.LogError("[RunUnBAR] - No TimeStamp Found! Decryption has failed.");
            }
        }
    }
}
