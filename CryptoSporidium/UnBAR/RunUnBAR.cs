using MultiServer.CryptoSporidium.BAR;
using MultiServer.Addons.ICSharpCode.SharpZipLib.Zip.Compression;
using MultiServer.Addons.ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using HttpMultipartParser;
using System.Text;

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

            if (data[0] == 0x00 && data[1] == 0x00 && data[2] == 0x00 && data[3] == 0x01)
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

                        byte[] IVA = AFSBLOWFISH.InitiateLUACryptoContext(EncryptedHeaderSHA1, byteSignatureIV);

                        if (IVA != null)
                        {
                            byte[] DecryptedSHA1 = AFSBLOWFISH.Crypt_Decrypt(EncryptedHeaderSHA1, IVA);

                            if (DecryptedSHA1 != null)
                            {
                                try
                                {
                                    // What to do with crypto is not clear yet.

                                    ServerConfiguration.LogInfo("Reference SHA1 - " + Misc.ByteArrayToHexString(DecryptedSHA1));

                                    // Create a new byte array to store the remaining content
                                    byte[] newFileBytes = new byte[data.Length - 24];

                                    // Copy the content after the first 24 bytes to the new array
                                    Array.Copy(data, 24, newFileBytes, 0, newFileBytes.Length);

                                    /*if (Misc.ByteArrayToHexString(DecryptedSHA1) == AFSMISC.ValidateSha1(newFileBytes))
                                        ServerConfiguration.LogInfo("[RunUnBar] - Protected file is OK.");
                                    else
                                        ServerConfiguration.LogWarn("[RunUnBar] - Protected file is not matching SHA1, abort.");*/
                                }
                                catch (Exception ex)
                                {
                                    ServerConfiguration.LogError($"[RunUnBAR] - EdgeZlib has failed with error {ex}");
                                }
                            }
                        }
                    }
                    else
                        ServerConfiguration.LogError("[RunUnBAR] - Encrypted data not found in BAR or false positive! Decryption has failed.");
                }
                else
                    ServerConfiguration.LogError("[RunUnBAR] - No TimeStamp Found! Decryption has failed.");
            }
        }

        public static byte[] Decompress(byte[] inData)
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

            // Token: 0x0400003B RID: 59
            internal ushort SourceSize;

            // Token: 0x0400003C RID: 60
            internal ushort CompressedSize;
        }
    }
}
