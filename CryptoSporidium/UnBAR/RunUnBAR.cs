using MultiServer.CryptoSporidium.BAR;
using MultiServer.Addons.ICSharpCode.SharpZipLib.Zip.Compression;
using MultiServer.Addons.ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace MultiServer.CryptoSporidium.UnBAR
{
    internal class RunUnBAR
    {
        public static void Run(string inputfile, string outputpath, bool edat, string options)
        {
            if (edat)
                RunDecrypt(inputfile, outputpath, options);
            else
                RunExtract(inputfile, outputpath, options);
        }

        public static void RunEncrypt(string filePath, string sdatfilePath, string sdatnpdcopyfile)
        {
            new EDAT().encryptFile(filePath, sdatfilePath, sdatnpdcopyfile);
        }

        private static void RunDecrypt(string filePath, string outDir, string options)
        {
            new EDAT().decryptFile(filePath, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath) + ".dat"));

            RunExtract(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath) + ".dat"), outDir, options);
        }

        private static void RunExtract(string filePath, string outDir, string options)
        {
            ServerConfiguration.LogInfo("Loading BAR/dat: {0}", filePath);

            byte[] RawBarData = null;

            if (File.Exists(filePath))
            {
                try
                {
                    RawBarData = File.ReadAllBytes(filePath);

                    byte[] HeaderIV = ExtractSHARCHeaderIV(RawBarData);

                    if (HeaderIV != null)
                    {
                        ServerConfiguration.LogInfo($"[RunExtract] - SHARC Header IV -> {Misc.ByteArrayToHexString(HeaderIV)}");

                        byte[] DecryptedTOC = AFSAES.InitiateCTRBuffer(ExtractSHARCHeader(RawBarData), Convert.FromBase64String(options), HeaderIV);

                        ServerConfiguration.LogInfo($"[RunExtract] - DECRYPTED SHARC Header -> {Misc.ByteArrayToHexString(DecryptedTOC)}");

                        byte[] EncryptedFileBytes = Misc.TrimStart(RawBarData, 52);

                        // File is decrypted using AES again, but a bit differently.
                    }
                    else
                    {
                        byte[] data = Misc.ReadBinaryFile(filePath, 0x0C, 4); // Read 4 bytes from offset 0x0C to 0x0F

                        string formattedData = BitConverter.ToString(data).Replace("-", ""); // Convert byte array to hex string

                        Directory.CreateDirectory(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)));

                        File.WriteAllText(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)) + "/timestamp.txt", formattedData);
                    }
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
                        ExtractToFileBarVersion1(RawBarData, archive, tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)), registeredExtension);
                    }
                    catch (Exception ex)
                    {
                        ServerConfiguration.LogWarn(ex.ToString());
                        string fileType = ".unknown";
                        ExtractToFileBarVersion1(RawBarData, archive, tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)), fileType);
                    }
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[RunUnBAR] - RunExtract Errored out - {ex}");
            }
        }

        public static byte[] ExtractSHARCHeaderIV(byte[] input)
        {
            // Check if the input has at least 24 bytes (8 for the pattern and 16 to copy)
            if (input.Length < 24)
            {
                ServerConfiguration.LogError("[ExtractSHARCHeaderIV] - Input byte array must have at least 24 bytes.");
                return null;
            }

            // Check if the first 8 bytes match the specified pattern
            byte[] pattern = new byte[] { 0xAD, 0xEF, 0x17, 0xE1, 0x02, 0x00, 0x00, 0x00 };
            for (int i = 0; i < 8; i++)
            {
                if (input[i] != pattern[i])
                {
                    ServerConfiguration.LogError("[ExtractSHARCHeaderIV] - The first 8 bytes do not match the SHARC pattern.");
                    return null;
                }
            }

            // Copy the next 16 bytes to a new array
            byte[] copiedBytes = new byte[16];
            Array.Copy(input, 8, copiedBytes, 0, copiedBytes.Length);

            return copiedBytes;
        }

        public static byte[] ExtractSHARCHeader(byte[] input)
        {
            // Check if the input has at least 52 bytes (8 for the pattern and 16 for the Header IV, and 28 for the Header)
            if (input.Length < 52)
            {
                ServerConfiguration.LogError("[ExtractSHARCHeader] - Input byte array must have at least 52 bytes.");
                return null;
            }

            // Check if the first 8 bytes match the specified pattern
            byte[] pattern = new byte[] { 0xAD, 0xEF, 0x17, 0xE1, 0x02, 0x00, 0x00, 0x00 };
            for (int i = 0; i < 8; i++)
            {
                if (input[i] != pattern[i])
                {
                    ServerConfiguration.LogError("[ExtractSHARCHeader] - The first 8 bytes do not match the SHARC pattern.");
                    return null;
                }
            }

            // Copy the next 28 bytes to a new array
            byte[] copiedBytes = new byte[28];
            Array.Copy(input, 24, copiedBytes, 0, copiedBytes.Length);

            return copiedBytes;
        }

        // Todo, separate the crypto implementation in the file designed for it.

        public static void ExtractToFileBarVersion1(byte[] RawBarData, BARArchive archive, HashedFileName FileName, string outDir, string fileType)
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

                        ServerConfiguration.LogInfo("[RunUnBAR] - Encrypted Content Detected!, Running Decryption.");
                        ServerConfiguration.LogInfo($"CompressedSize - {compressedSize}");
                        ServerConfiguration.LogInfo($"Size - {fileSize}");
                        ServerConfiguration.LogInfo($"dataStart - 0x{dataStart:X}");
                        ServerConfiguration.LogInfo($"UserData - 0x{userData:X}");

                        byte[] SignatureIV = BitConverter.GetBytes(AFSMISC.BuildSignatureIv((int)fileSize, (int)compressedSize, dataStart, userData));

                        // If you want to ensure little-endian byte order explicitly, you can reverse the array
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(SignatureIV);

                        data = AFSMISC.RemovePaddingPrefix(data);

                        byte[] EncryptedHeaderSHA1 = new byte[24];

                        // Copy the first 24 bytes from the source array to the destination array
                        Array.Copy(data, 0, EncryptedHeaderSHA1, 0, EncryptedHeaderSHA1.Length);

                        byte[] SHA1DATA = AFSBLOWFISH.EncryptionProxyInit(EncryptedHeaderSHA1, SignatureIV);

                        if (SHA1DATA != null)
                        {
                            string verificationsha1 = Misc.ByteArrayToHexString(SHA1DATA);

                            // Create a new byte array to store the remaining content
                            byte[] FileBytes = new byte[data.Length - 24];

                            // Copy the content after the first 24 bytes to the new array
                            Array.Copy(data, 24, FileBytes, 0, FileBytes.Length);

                            string sha1 = AFSMISC.ValidateSha1(FileBytes);

                            if (sha1 == verificationsha1.Substring(0, verificationsha1.Length - 8))
                            {
                                int newvalue = SignatureIV[7] += 3;

                                SignatureIV[7] = (byte)newvalue;

                                FileBytes = Decompress(AFSBLOWFISH.EncryptionProxyProcessMemory(FileBytes, SignatureIV));

                                fileStream.Write(FileBytes, 0, FileBytes.Length);
                                fileStream.Close();
                            }
                            else
                            {
                                ServerConfiguration.LogWarn($"[RunUnBAR] - Lua file (SHA1 - {sha1}) has been tempered with! (Reference SHA1 - {verificationsha1.Substring(0, verificationsha1.Length - 8)}), Aborting decryption.");
                                fileStream.Write(data, 0, data.Length);
                                fileStream.Close();
                            }
                        }
                    }
                    else
                    {
                        ServerConfiguration.LogError("[RunUnBAR] - Encrypted data not found in BAR or false positive! Decryption has failed.");
                        fileStream.Write(data, 0, data.Length);
                        fileStream.Close();
                    }
                }
                else
                {
                    ServerConfiguration.LogError("[RunUnBAR] - No TimeStamp Found! Decryption has failed.");
                    fileStream.Write(data, 0, data.Length);
                    fileStream.Close();
                }
            }
            else
            {
                fileStream.Write(data, 0, data.Length);
                fileStream.Close();
            }
            ServerConfiguration.LogInfo("Extracted file {0}", new object[1]
            {
                 Path.GetFileName(path)
            });
        }

        private static byte[] Decompress(byte[] inData)
        {
            MemoryStream memoryStream = new MemoryStream();
            MemoryStream memoryStream2 = new MemoryStream(inData);
            byte[] array = new byte[ChunkHeader.SizeOf];
            while (memoryStream2.Position < memoryStream2.Length)
            {
                memoryStream2.Read(array, 0, ChunkHeader.SizeOf);
                array = Utils.EndianSwap(array);
                ChunkHeader header = ChunkHeader.FromBytes(array);
                int compressedSize = header.CompressedSize;
                byte[] array2 = new byte[compressedSize];
                memoryStream2.Read(array2, 0, compressedSize);
                byte[] array3 = DecompressChunk(array2, header);
                memoryStream.Write(array3, 0, array3.Length);
            }
            memoryStream2.Close();
            memoryStream.Close();
            return memoryStream.ToArray();
        }

        private static byte[] DecompressChunk(byte[] inData, ChunkHeader header)
        {
            if (header.CompressedSize == header.SourceSize)
                return inData;
            MemoryStream baseInputStream = new MemoryStream(inData);
            Inflater inf = new Inflater(true);
            InflaterInputStream inflaterInputStream = new InflaterInputStream(baseInputStream, inf);
            MemoryStream memoryStream = new MemoryStream();
            byte[] array = new byte[4096];
            for (; ; )
            {
                int num = inflaterInputStream.Read(array, 0, array.Length);
                if (num <= 0)
                    break;
                memoryStream.Write(array, 0, num);
            }
            inflaterInputStream.Close();
            return memoryStream.ToArray();
        }

        internal struct ChunkHeader
        {
            internal byte[] GetBytes()
            {
                byte[] array = new byte[4];
                Array.Copy(BitConverter.GetBytes(SourceSize), 0, array, 2, 2);
                Array.Copy(BitConverter.GetBytes(CompressedSize), 0, array, 0, 2);
                return array;
            }

            internal static int SizeOf
            {
                get
                {
                    return 4;
                }
            }

            internal static ChunkHeader FromBytes(byte[] inData)
            {
                ChunkHeader result = default;
                byte[] array = inData;
                if (inData.Length > SizeOf)
                {
                    array = new byte[4];
                    Array.Copy(inData, array, 4);
                }
                result.SourceSize = BitConverter.ToUInt16(array, 2);
                result.CompressedSize = BitConverter.ToUInt16(array, 0);
                return result;
            }

            internal ushort SourceSize;

            internal ushort CompressedSize;
        }
    }
}
