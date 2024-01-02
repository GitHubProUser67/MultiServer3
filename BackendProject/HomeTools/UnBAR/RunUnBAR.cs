using CustomLogger;
using BackendProject.HomeTools.BARFramework;
using BackendProject.HomeTools.Crypto;

namespace BackendProject.HomeTools.UnBAR
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
            bool isSharc = false;
            bool isLittleEndian = false;
            byte[]? RawBarData = null;

            if (File.Exists(filePath))
            {
                try
                {
                    RawBarData = File.ReadAllBytes(filePath);

                    if (MiscUtils.FindBytePattern(RawBarData, new byte[] { 0xAD, 0xEF, 0x17, 0xE1, 0x02, 0x00, 0x00, 0x00 }) != -1)
                        isSharc = true;
                    else if (MiscUtils.FindBytePattern(RawBarData, new byte[] { 0xE1, 0x17, 0xEF, 0xAD, 0x00, 0x00, 0x00, 0x02 }) != -1)
                    {
                        isSharc = true;
                        isLittleEndian = true;
                    }

                    if (isSharc && RawBarData.Length > 52)
                    {
                        AESCTR256EncryptDecrypt? aes256 = new();
                        ToolsImpl? toolsImpl = new();

                        try
                        {
                            byte[]? HeaderIV = new byte[16];

                            Buffer.BlockCopy(RawBarData, 8, HeaderIV, 0, HeaderIV.Length);

                            if (HeaderIV != null)
                            {
                                byte[] SharcHeader = new byte[28];

                                Buffer.BlockCopy(RawBarData, 24, SharcHeader, 0, SharcHeader.Length);

                                SharcHeader = aes256.InitiateCTRBuffer(SharcHeader,
                                 Convert.FromBase64String(options), HeaderIV);

                                Console.WriteLine(MiscUtils.ByteArrayToHexString(HeaderIV));

                                if (SharcHeader != Array.Empty<byte>())
                                {
                                    byte[] NumOfFiles = new byte[4];

                                    if (isLittleEndian == true)
                                        Buffer.BlockCopy(SharcHeader, SharcHeader.Length - 20, NumOfFiles, 0, NumOfFiles.Length);
                                    else
                                        Buffer.BlockCopy(Org.BouncyCastle.util.EndianTools.ReverseEndiannessInChunks(SharcHeader, 4), SharcHeader.Length - 20, NumOfFiles, 0, NumOfFiles.Length);

                                    byte[]? SharcTOC = new byte[24 * BitConverter.ToUInt32(NumOfFiles, 0)];

                                    Buffer.BlockCopy(RawBarData, 52, SharcTOC, 0, SharcTOC.Length);

                                    if (SharcTOC != null)
                                    {
                                        byte[] OriginalIV = new byte[HeaderIV.Length];

                                        Buffer.BlockCopy(HeaderIV, 0, OriginalIV, 0, OriginalIV.Length);

                                        toolsImpl.IncrementIVBytes(HeaderIV, 1); // IV so we increment.

                                        SharcTOC = aes256.InitiateCTRBuffer(SharcTOC, Convert.FromBase64String(options), HeaderIV);

                                        if (SharcTOC != Array.Empty<byte>())
                                        {
                                            byte[] SharcData = new byte[RawBarData.Length - (52 + SharcTOC.Length)];

                                            Buffer.BlockCopy(RawBarData, 52 + SharcTOC.Length, SharcData, 0, SharcData.Length);

                                            byte[] FileBytes = new byte[0];

                                            if (isLittleEndian)
                                            {
                                                FileBytes = MiscUtils.CombineByteArrays(new byte[] { 0xE1, 0x17, 0xEF, 0xAD, 0x00, 0x00, 0x00, 0x02 }, new byte[][]
                                                {
                                                    OriginalIV,
                                                    SharcHeader,
                                                    SharcTOC,
                                                    SharcData
                                                });
                                            }
                                            else
                                            {
                                                FileBytes = MiscUtils.CombineByteArrays(new byte[] { 0xAD, 0xEF, 0x17, 0xE1, 0x02, 0x00, 0x00, 0x00 }, new byte[][]
                                                {
                                                    OriginalIV,
                                                    SharcHeader,
                                                    SharcTOC,
                                                    SharcData
                                                });
                                            }

                                            Directory.CreateDirectory(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)));

                                            File.WriteAllBytes(filePath, FileBytes);

                                            if (isLittleEndian)
                                            {
                                                File.WriteAllText(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)) + "/timestamp.txt",
                                                BitConverter.ToString(Org.BouncyCastle.util.EndianTools.ReverseEndiannessInChunks(new byte[] { FileBytes[28], FileBytes[29], FileBytes[30], FileBytes[31] }, 4)).Replace("-", ""));
                                            }
                                            else
                                            {
                                                File.WriteAllText(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)) + "/timestamp.txt",
                                                BitConverter.ToString(new byte[] { FileBytes[28], FileBytes[29], FileBytes[30], FileBytes[31] }).Replace("-", ""));
                                            }

                                            LoggerAccessor.LogInfo("Loading SHARC/dat: {0}", filePath);
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
                        if (MiscUtils.FindBytePattern(RawBarData, new byte[] { 0xAD, 0xEF, 0x17, 0xE1 }) != -1)
                        {

                        }
                        else if (MiscUtils.FindBytePattern(RawBarData, new byte[] { 0xE1, 0x17, 0xEF, 0xAD }) != -1)
                            isLittleEndian = true;
                        else
                            return; // File not a BAR.

                        Directory.CreateDirectory(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)));

                        if (isLittleEndian)
                        {
                            File.WriteAllText(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)) + "/timestamp.txt",
                            BitConverter.ToString(Org.BouncyCastle.util.EndianTools.ReverseEndiannessInChunks(new byte[] { RawBarData[12], RawBarData[13], RawBarData[14], RawBarData[15] }, 4)).Replace("-", ""));
                        }
                        else
                        {
                            File.WriteAllText(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)) + "/timestamp.txt",
                            BitConverter.ToString(new byte[] { RawBarData[12], RawBarData[13], RawBarData[14], RawBarData[15] }).Replace("-", ""));
                        }

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
                    archive = new(filePath, outDir);
                    archive.Load();
                    archive.WriteMap(filePath);

                    // Create a list to hold the tasks
                    List<Task>? TOCTasks = new();

                    foreach (TOCEntry tableOfContent in archive.TableOfContents)
                    {
                        byte[] FileData = tableOfContent.GetData(archive.GetHeader().Flags);

                        if (FileData != null)
                        {
                            // Create a task for each iteration
                            Task task = Task.Run(() =>
                            {
                                try
                                {
                                    if (archive.GetHeader().Version == 512)
                                        ExtractToFileBarVersion2(archive.GetHeader().Key, archive, tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)));
                                    else
                                    {
                                        using (MemoryStream memoryStream = new(FileData))
                                        {
                                            string registeredExtension = FileTypeAnalyser.Instance.GetRegisteredExtension(FileTypeAnalyser.Instance.Analyse(memoryStream));
                                            ExtractToFileBarVersion1(RawBarData, archive, tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)), registeredExtension);
                                            memoryStream.Flush();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LoggerAccessor.LogWarn(ex.ToString());
                                    if (archive.GetHeader().Version == 512)
                                        ExtractToFileBarVersion2(archive.GetHeader().Key, archive, tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)));
                                    else
                                        ExtractToFileBarVersion1(RawBarData, archive, tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)), ".unknown");
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
                    else if (filePath.Length > 4 && File.Exists(filePath[..^4] + ".sharc.map"))
                        File.Move(filePath[..^4] + ".sharc.map", Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath) + $"/{Path.GetFileName(filePath)}.map"));
                    else if (filePath.Length > 4 && File.Exists(filePath[..^4] + ".bar.map"))
                        File.Move(filePath[..^4] + ".bar.map", Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath) + $"/{Path.GetFileName(filePath)}.map"));
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RunUnBAR] - RunExtract Errored out - {ex}");
            }
        }

        public void ExtractToFileBarVersion1(byte[]? RawBarData, BARArchive archive, HashedFileName FileName, string outDir, string fileType)
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
                    if (tableOfContent.Compression == CompressionMethod.Encrypted && data.Length > 28 && 
                        ((data[0] == 0x00 && data[1] == 0x00 && data[2] == 0x00 && data[3] == 0x01) || (data[0] == 0x01 && data[1] == 0x00 && data[2] == 0x00 && data[3] == 0x00)))
                    {
                        if (File.Exists(outDir + "/timestamp.txt") && RawBarData != null)
                        {
                            int dataStart = FindDataPosInBinary(RawBarData, data);

                            if (dataStart != -1)
                            {
                                BlowfishCTREncryptDecrypt? blowfish = new();
                                ToolsImpl? toolsImpl = new();
                                uint compressedSize = tableOfContent.CompressedSize;
                                uint fileSize = tableOfContent.Size;
                                int userData = Utils.EndianSwap(BitConverter.ToInt32(MiscUtils.HexStringToByteArray(File.ReadAllText(outDir + "/timestamp.txt"))));
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

                                byte[] EncryptedHeaderSHA1 = new byte[24];

                                // Copy the first 24 bytes from the source array to the destination array
                                Buffer.BlockCopy(data, 4, EncryptedHeaderSHA1, 0, EncryptedHeaderSHA1.Length);

                                byte[]? DecryptedHeaderSHA1 = blowfish.EncryptionProxyInit(EncryptedHeaderSHA1, SignatureIV);

                                if (DecryptedHeaderSHA1 != null)
                                {
                                    string verificationsha1 = MiscUtils.ByteArrayToHexString(DecryptedHeaderSHA1);
#if DEBUG
                                    LoggerAccessor.LogInfo($"SignatureHeader - {verificationsha1}");
#endif
                                    // Create a new byte array to store the remaining content
                                    byte[]? FileBytes = new byte[data.Length - 28];

                                    // Copy the content after the first 28 bytes to the new array
                                    Array.Copy(data, 28, FileBytes, 0, FileBytes.Length);

                                    string sha1 = toolsImpl.ValidateSha1(FileBytes);

                                    if (sha1 == verificationsha1[..^8]) // We strip the original file Compression size.
                                    {
                                        toolsImpl.IncrementIVBytes(SignatureIV, 3);

                                        FileBytes = blowfish.InitiateCTRBuffer(FileBytes, SignatureIV);

                                        if (FileBytes != null)
                                        {
                                            try
                                            {
                                                FileBytes = toolsImpl.ComponentAceEdgeZlibDecompress(FileBytes);
                                            }
                                            catch (Exception ex)
                                            {
                                                LoggerAccessor.LogError($"[RunUnBar] - Errored out when processing Encryption Proxy encrypted content - {ex}");
                                                FileBytes = data;
                                            }

                                            fileStream.Write(FileBytes, 0, FileBytes.Length);
                                            fileStream.Close();
                                        }
                                        else
                                        {
                                            LoggerAccessor.LogWarn($"[RunUnBAR] - Encrypted file failed to decrypt, Writing original data.");
                                            fileStream.Write(data, 0, data.Length);
                                            fileStream.Close();
                                        }
                                    }
                                    else
                                    {
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

        public void ExtractToFileBarVersion2(byte[] Key, BARArchive archive, HashedFileName FileName, string outDir)
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
                LoggerAccessor.LogInfo($"Key - {MiscUtils.ByteArrayToHexString(Key)}");
                LoggerAccessor.LogInfo($"IV - {MiscUtils.ByteArrayToHexString(tableOfContent.IV)}");
#endif
                // Why we not use the ICSharp version, it turns out, there is 2 variant of edgezlib. ICSharp mostly handle the older type.
                // While packing sharc, original script used the zlib1.dll, which we have in c# equivalent, so we use it.
                // Weird issues can happen when decompressing some files if not using this version.

                byte[]? FileBytes = null;

                try
                {
                    FileBytes = toolsImpl.ComponentAceEdgeZlibDecompress(toolsImpl.ProcessXTEABlocksAsync(data, Key, tableOfContent.IV));
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[RunUnBar] - Errored out when processing XTEA Proxy encrypted content - {ex}");
                    FileBytes = data;
                }

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

        private static byte[]? ExtractNumOfFilesBigEndian(byte[] input)
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
            result = Org.BouncyCastle.util.EndianTools.ReverseEndiannessInChunks(result, 4);

            return result;
        }


        private static int FindDataPosInBinary(byte[] data1, byte[] data2)
        {
            for (int i = 0; i < data1.Length - data2.Length + 1; i++)
            {
                bool found = true;
                for (int j = 0; j < data2.Length; j++)
                {
                    if (data1[i + j] != data2[j])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                    return i;
            }

            return -1; // Data2 not found in Data1
        }
    }
}
