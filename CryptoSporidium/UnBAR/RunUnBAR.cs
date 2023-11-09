using CustomLogger;
using CryptoSporidium.BAR;

namespace CryptoSporidium.UnBAR
{
    public class RunUnBAR
    {
        public async Task Run(string inputfile, string outputpath, bool edat, string options)
        {
            RunUnBAR? run = new();

            if (edat)
                await RunDecrypt(inputfile, outputpath, options);
            else
                await run.RunExtract(inputfile, outputpath, options);

            run = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect(); // We have no choice and it's not possible to remove them, HomeTools create a BUNCH of necessary objects.
        }

        public void RunEncrypt(string filePath, string sdatfilePath, string? sdatnpdcopyfile)
        {
            try
            {
                new EDAT().encryptFile(filePath, sdatfilePath, sdatnpdcopyfile);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RunEncrypt] - SDAT Encryption failed with exception : {ex}");
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect(); // We have no choice and it's not possible to remove them, HomeTools create a BUNCH of necessary objects.
        }

        private async Task RunDecrypt(string filePath, string outDir, string options)
        {
            EDAT? edatInstance = new();

            RunUnBAR? run = new();

            int status = edatInstance.decryptFile(filePath, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath) + ".dat"));

            if (status == 0)
                await run.RunExtract(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath) + ".dat"), outDir, options);
            else
                LoggerAccessor.LogError($"[RunUnBAR] - RunDecrypt failed with status code : {status}");

            edatInstance = null;

            run = null;
        }

        private async Task RunExtract(string filePath, string outDir, string options)
        {
            bool isSharc = true;
            byte[]? RawBarData = null;
            MiscUtils? utils = new();

            if (File.Exists(filePath))
            {
                try
                {
                    RawBarData = File.ReadAllBytes(filePath);
                    byte[] Magic = new byte[] { 0xAD, 0xEF, 0x17, 0xE1 };
                    // Check if the first 8 bytes match the specified pattern
                    byte[] pattern = utils.Combinebytearay(Magic, new byte[] { 0x02, 0x00, 0x00, 0x00 });
                    for (int i = 0; i < 8; i++)
                    {
                        if (RawBarData[i] != pattern[i])
                        {
                            isSharc = false;
                            break;
                        }
                    }
                    if (isSharc)
                    {
                        AESCTR256EncryptDecrypt? aes256 = new();
                        ToolsImpl? toolsImpl = new();

                        try
                        {
                            byte[]? HeaderIV = ExtractSHARCHeaderIV(RawBarData);

                            if (HeaderIV != null)
                            {
                                byte[]? DecryptedHeader = aes256.InitiateCTRBuffer(ExtractEncryptedSharcHeaderData(RawBarData),
                                 Convert.FromBase64String(options), HeaderIV);

                                if (DecryptedHeader != null)
                                {
                                    toolsImpl.IncrementIVBytes(HeaderIV, 1); // IV so we increment.

                                    byte[]? NumOfFiles = ExtractNumOfFiles(DecryptedHeader);

                                    if (NumOfFiles != null)
                                    {
                                        uint TOCSize = 24 * BitConverter.ToUInt32(NumOfFiles, 0);

                                        byte[]? EncryptedTOC = utils.CopyBytes(utils.TrimStart(RawBarData, 52), TOCSize);

                                        if (EncryptedTOC != null)
                                        {
                                            byte[]? DecryptedTOC = aes256.InitiateCTRBuffer(utils.CopyBytes(utils.TrimStart(RawBarData, 52), TOCSize), Convert.FromBase64String(options), HeaderIV);

                                            if (DecryptedTOC != null)
                                            {
                                                byte[] FileBytes = utils.Combinebytearay(pattern,
                                                utils.Combinebytearay(HeaderIV, utils.Combinebytearay(DecryptedHeader,
                                                utils.Combinebytearay(DecryptedTOC, utils.TrimBytes(utils.TrimStart(RawBarData, 52), TOCSize)))));

                                                Directory.CreateDirectory(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)));

                                                File.WriteAllBytes(filePath, FileBytes);

                                                File.WriteAllText(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)) + "/timestamp.txt",
                                                    BitConverter.ToString(utils.ReadBinaryFile(filePath, 0x1C, 4)).Replace("-", ""));

                                                LoggerAccessor.LogInfo("Loading SHARC/dat: {0}", filePath);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerAccessor.LogError($"[RunUnBAR] - SHARC Decryption failed! with error - {ex}");
                        }

                        aes256 = null;
                        toolsImpl = null;
                    }
                    else
                    {
                        Magic = Org.BouncyCastle.util.EndianTools.ReverseEndiannessInChunks(Magic, 4);
                        for (int i = 0; i < 4; i++)
                        {
                            if (RawBarData[i] != Magic[i])
                                return; // File not a bar, we return.
                        }

                        Directory.CreateDirectory(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)));

                        File.WriteAllText(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)) + "/timestamp.txt",
                            BitConverter.ToString(utils.ReadBinaryFile(filePath, 0x0C, 4)).Replace("-", ""));

                        LoggerAccessor.LogInfo("Loading BAR/dat: {0}", filePath);
                    }
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[RunUnBAR] - Timestamp creation failed! with error - {ex}");
                }
            }

            try
            {
                if (File.Exists(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)) + "/timestamp.txt"))
                {
                    BARArchive? archive = null;
                    if (isSharc)
                        archive = new(filePath, outDir, true);
                    else
                        archive = new(filePath, outDir);
                    archive.Load();
                    archive.WriteMap(filePath);

                    // Create a list to hold the tasks
                    List<Task>? TOCTasks = new List<Task>();

                    foreach (TOCEntry tableOfContent in archive.TableOfContents)
                    {
                        byte[] FileData = tableOfContent.GetData(archive.GetHeader().Flags);

                        if (FileData != null)
                        {
                            // Create a task for each iteration
                            Task task = Task.Run(async () =>
                            {
                                try
                                {
                                    if (archive.GetHeader().Version == 512)
                                        await ExtractToFileBarVersion2(archive.GetHeader().Key, archive, tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)), utils);
                                    else
                                    {
                                        using (MemoryStream memoryStream = new(FileData))
                                        {
                                            string registeredExtension = FileTypeAnalyser.Instance.GetRegisteredExtension(FileTypeAnalyser.Instance.Analyse(memoryStream));
                                            ExtractToFileBarVersion1(RawBarData, archive, tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)), registeredExtension, utils);
                                            memoryStream.Flush();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LoggerAccessor.LogWarn(ex.ToString());
                                    if (archive.GetHeader().Version == 512)
                                        await ExtractToFileBarVersion2(archive.GetHeader().Key, archive, tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)), utils);
                                    else
                                        ExtractToFileBarVersion1(RawBarData, archive, tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)), ".unknown", utils);
                                }
                            });

                            TOCTasks.Add(task);
                        }
                    }

                    // Wait for all tasks to complete
                    await Task.WhenAll(TOCTasks);

                    TOCTasks = null;
                    archive = null;

                    if (File.Exists(filePath + ".map"))
                        File.Move(filePath + ".map", Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath) + $"/{Path.GetFileName(filePath)}.map"));
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RunUnBAR] - RunExtract Errored out - {ex}");
            }

            utils = null;
        }

        public void ExtractToFileBarVersion1(byte[]? RawBarData, BARArchive archive, HashedFileName FileName, string outDir, string fileType, MiscUtils utils)
        {
            TOCEntry? tableOfContent = archive.TableOfContents[FileName];
            string path = string.Empty;
            if (string.IsNullOrEmpty(tableOfContent.Path))
                path = string.Format("{0}{1}{2:X8}{3}", outDir, Path.DirectorySeparatorChar, FileName.Value, fileType);
            else
            {
                string str = tableOfContent.Path.Replace('/', Path.DirectorySeparatorChar);
                path = string.Format("{0}{1}{2}", outDir, Path.DirectorySeparatorChar, str);
            }
            string? outdirectory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(outdirectory))
            {
                Directory.CreateDirectory(outdirectory);
                using (FileStream fileStream = File.Open(path, (FileMode)2))
                {
                    byte[] data = tableOfContent.GetData(archive.GetHeader().Flags);
                    if (tableOfContent.Compression == CompressionMethod.Encrypted && data.Length > 28 && data[0] == 0x00 && data[1] == 0x00 && data[2] == 0x00 && data[3] == 0x01)
                    {
                        if (File.Exists(outDir + "/timestamp.txt") && RawBarData != null)
                        {
                            int dataStart = utils.FindDataPosInBinary(RawBarData, data);

                            if (dataStart != -1)
                            {
                                BlowfishCTREncryptDecrypt? blowfish = new();
                                ToolsImpl? toolsImpl = new();
                                uint compressedSize = tableOfContent.CompressedSize;
                                uint fileSize = tableOfContent.Size;
                                int userData = BitConverter.ToInt32(utils.HexStringToByteArray(File.ReadAllText(outDir + "/timestamp.txt")));
#if DEBUG
                                LoggerAccessor.LogInfo("[RunUnBAR] - Encrypted Content Detected!, Running Decryption.");
                                LoggerAccessor.LogInfo($"CompressedSize - {compressedSize}");
                                LoggerAccessor.LogInfo($"Size - {fileSize}");
                                LoggerAccessor.LogInfo($"dataStart - 0x{dataStart:X}");
                                LoggerAccessor.LogInfo($"UserData - 0x{userData:X}");
#endif
                                byte[] SignatureIV = BitConverter.GetBytes(toolsImpl.BuildSignatureIv((int)fileSize, (int)compressedSize, dataStart, userData));

                                // If you want to ensure little-endian byte order explicitly, you can reverse the array
                                if (BitConverter.IsLittleEndian)
                                    Array.Reverse(SignatureIV);

                                data = toolsImpl.RemovePaddingPrefix(data);

                                byte[] EncryptedHeaderSHA1 = new byte[24];

                                // Copy the first 24 bytes from the source array to the destination array
                                Array.Copy(data, 0, EncryptedHeaderSHA1, 0, EncryptedHeaderSHA1.Length);

                                byte[]? DecryptedHeaderSHA1 = blowfish.EncryptionProxyInit(EncryptedHeaderSHA1, SignatureIV);

                                if (DecryptedHeaderSHA1 != null)
                                {
                                    string verificationsha1 = utils.ByteArrayToHexString(DecryptedHeaderSHA1);
#if DEBUG
                                    LoggerAccessor.LogInfo($"SignatureHeader - {verificationsha1}");
#endif
                                    // Create a new byte array to store the remaining content
                                    byte[]? FileBytes = new byte[data.Length - 24];

                                    // Copy the content after the first 24 bytes to the new array
                                    Array.Copy(data, 24, FileBytes, 0, FileBytes.Length);

                                    string sha1 = toolsImpl.ValidateSha1(FileBytes);

                                    if (sha1 == verificationsha1.Substring(0, verificationsha1.Length - 8)) // We strip the original file Compression size.
                                    {
                                        toolsImpl.IncrementIVBytes(SignatureIV, 3);

                                        FileBytes = blowfish.InitiateCTRBuffer(FileBytes, SignatureIV);

                                        if (FileBytes != null)
                                        {
                                            try
                                            {
                                                FileBytes = toolsImpl.ICSharpEdgeZlibDecompress(FileBytes);
                                            }
                                            catch (Exception ex)
                                            {
                                                LoggerAccessor.LogError($"[RunUnBar] - Errored out when processing Encryption Proxy encrypted content - {ex}");
                                                FileBytes = toolsImpl.ApplyPaddingPrefix(data);
                                            }

                                            if (FileBytes == null)
                                                FileBytes = toolsImpl.ApplyPaddingPrefix(data);

                                            fileStream.Write(FileBytes, 0, FileBytes.Length);
                                            fileStream.Close();
                                        }
                                        else
                                        {
                                            data = toolsImpl.ApplyPaddingPrefix(data);
                                            LoggerAccessor.LogWarn($"[RunUnBAR] - Encrypted file failed to decrypt, Writing original data.");
                                            fileStream.Write(data, 0, data.Length);
                                            fileStream.Close();
                                        }
                                    }
                                    else
                                    {
                                        data = toolsImpl.ApplyPaddingPrefix(data);
                                        LoggerAccessor.LogWarn($"[RunUnBAR] - Encrypted file (SHA1 - {sha1}) has been tempered with! (Reference SHA1 - {verificationsha1.Substring(0, verificationsha1.Length - 8)}), Aborting decryption.");
                                        fileStream.Write(data, 0, data.Length);
                                        fileStream.Close();
                                    }
                                }

                                blowfish = null;
                                toolsImpl = null;
                            }
                            else
                            {
                                LoggerAccessor.LogError("[RunUnBAR] - Encrypted data not found in BAR or false positive! Decryption has failed.");
                                fileStream.Write(data, 0, data.Length);
                                fileStream.Close();
                            }
                        }
                        else
                        {
                            LoggerAccessor.LogError("[RunUnBAR] - No TimeStamp Found! Decryption has failed.");
                            fileStream.Write(data, 0, data.Length);
                            fileStream.Close();
                        }
                    }
                    else
                    {
                        fileStream.Write(data, 0, data.Length);
                        fileStream.Close();
                    }
                }
#if DEBUG
                LoggerAccessor.LogInfo("Extracted file {0}", new object[1]
                {
                    Path.GetFileName(path)
                });
#endif
            }

            tableOfContent = null;
        }

        public async Task ExtractToFileBarVersion2(byte[] Key, BARArchive archive, HashedFileName FileName, string outDir, MiscUtils utils)
        {
            TOCEntry? tableOfContent = archive.TableOfContents[FileName];
            ToolsImpl? toolsImpl = new();
            string path = string.Empty;
            if (!string.IsNullOrEmpty(tableOfContent.Path))
            {
                string str = tableOfContent.Path.Replace('/', Path.DirectorySeparatorChar);
                path = string.Format("{0}{1}{2}", outDir, Path.DirectorySeparatorChar, str);
            }
            byte[] data = tableOfContent.GetData(archive.GetHeader().Flags);
            if (tableOfContent.Compression == CompressionMethod.Encrypted)
            {
#if DEBUG
                LoggerAccessor.LogInfo("[RunUnBAR] - Encrypted Content Detected!, Running Decryption.");
                LoggerAccessor.LogInfo($"Key - {utils.ByteArrayToHexString(Key)}");
                LoggerAccessor.LogInfo($"IV - {utils.ByteArrayToHexString(tableOfContent.IV)}");
#endif
                // Why we not use the ICSharp version, it turns out, there is 2 variant of edgezlib. ICSharp mostly handle the older type.
                // While packing sharc, original script used the zlib1.dll, which we have in c# equivalent, so we use it.
                // Weird issues can happen when decompressing some files if not using this version.

                byte[]? FileBytes = null;

                try
                {
                    FileBytes = toolsImpl.ComponentAceEdgeZlibDecompress(await toolsImpl.ProcessXTEABlocksAsync(data, Key, tableOfContent.IV));
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[RunUnBar] - Errored out when processing XTEA Proxy encrypted content - {ex}");
                    FileBytes = data;
                }

                if (FileBytes == null)
                    FileBytes = data;

                using (MemoryStream memoryStream = new(FileBytes))
                {
                    string registeredExtension = string.Empty;

                    try
                    {
                        registeredExtension = FileTypeAnalyser.Instance.GetRegisteredExtension(FileTypeAnalyser.Instance.Analyse(memoryStream));
                    }
                    catch (Exception)
                    {
                        registeredExtension = ".unknown";
                    }

                    if (path == string.Empty)
                        path = string.Format("{0}{1}{2:X8}{3}", outDir, Path.DirectorySeparatorChar, FileName.Value, registeredExtension);

                    string? outdirectory = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(outdirectory))
                    {
                        Directory.CreateDirectory(outdirectory);

                        using (FileStream fileStream = File.Open(path, (FileMode)2))
                        {
                            fileStream.Write(FileBytes, 0, FileBytes.Length);
                            fileStream.Close();
                        }
                    }

                    memoryStream.Flush();
                }
            }
            else
            {
                using (MemoryStream memoryStream = new(data))
                {
                    string registeredExtension = string.Empty;

                    try
                    {
                        registeredExtension = FileTypeAnalyser.Instance.GetRegisteredExtension(FileTypeAnalyser.Instance.Analyse(memoryStream));
                    }
                    catch (Exception)
                    {
                        registeredExtension = ".unknown";
                    }

                    if (path == string.Empty)
                        path = string.Format("{0}{1}{2:X8}{3}", outDir, Path.DirectorySeparatorChar, FileName.Value, registeredExtension);

                    string? outdirectory = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(outdirectory))
                    {
                        Directory.CreateDirectory(outdirectory);

                        using (FileStream fileStream = File.Open(path, (FileMode)2))
                        {
                            fileStream.Write(data, 0, data.Length);
                            fileStream.Close();
                        }
                    }

                    memoryStream.Flush();
                }
            }
#if DEBUG
            LoggerAccessor.LogInfo("Extracted file {0}", new object[1]
            {
                Path.GetFileName(path)
            });
#endif
            tableOfContent = null;
            toolsImpl = null;
        }

        public static byte[]? ExtractSHARCHeaderIV(byte[] input)
        {
            // Check if the input has at least 24 bytes (8 for the pattern and 16 to copy)
            if (input == null || input.Length < 24)
            {
                LoggerAccessor.LogError("[ExtractSHARCHeaderIV] - Input byte array must have at least 24 bytes.");
                return null;
            }

            // Copy the next 16 bytes to a new array
            byte[] copiedBytes = new byte[16];
            Array.Copy(input, 8, copiedBytes, 0, copiedBytes.Length);

            return copiedBytes;
        }

        public static byte[]? ExtractEncryptedSharcHeaderData(byte[] input)
        {
            // Check if the input has at least 52 bytes (8 for the pattern and 16 for the Header IV, and 28 for the Header)
            if (input == null || input.Length < 52)
            {
                LoggerAccessor.LogError("[ExtractEncryptedSharcHeaderData] - Input byte array must have at least 52 bytes.");
                return null;
            }

            // Copy the next 28 bytes to a new array
            byte[] copiedBytes = new byte[28];
            Array.Copy(input, 24, copiedBytes, 0, copiedBytes.Length);

            return copiedBytes;
        }

        public static byte[]? ExtractNumOfFiles(byte[] input)
        {
            if (input == null || input.Length < 20)
            {
                LoggerAccessor.LogError("[ExtractNumOfFiles] - Input byte array must have at least 20 bytes.");
                return null;
            }

            // Create a new byte array to store the first 4 bytes from the last 20 bytes.
            byte[] result = new byte[4];
            // Copy the first 4 bytes from the last 20 bytes into the result array.
            Array.Copy(input, input.Length - 20, result, 0, 4);
            if (BitConverter.IsLittleEndian)
                result = Org.BouncyCastle.util.EndianTools.ReverseEndiannessInChunks(result, 4);

            return result;
        }
    }
}
