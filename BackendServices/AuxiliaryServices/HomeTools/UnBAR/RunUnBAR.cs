using CustomLogger;
using HomeTools.BARFramework;
using HomeTools.Crypto;
using System.Diagnostics;
using System.Text;
using System.Security.Cryptography;
using EndianTools;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using CyberBackendLibrary.Crypto;
using CyberBackendLibrary.DataTypes;

namespace HomeTools.UnBAR
{
    public static class RunUnBAR
    {
        public static async Task Run(string converterPath, string filePath, string outputpath, bool edat)
        {
            if (edat)
                await RunDecrypt(converterPath, filePath, outputpath);
            else
                await RunExtract(filePath, outputpath);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect(); // We have no choice and it's not possible to remove them, HomeTools create a BUNCH of necessary objects.
        }

        public static void RunEncrypt(string filePath, string sdatfilePath)
        {
            try
            {
                int statuscode = new EDAT().encryptFile(filePath, sdatfilePath);

                if (statuscode != 0)
                    LoggerAccessor.LogError($"[RunUnBAR] - RunEncrypt failed with status code : {statuscode}");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RunEncrypt] - SDAT Encryption failed with exception : {ex}");
            }
        }

        private static async Task RunDecrypt(string converterPath, string sdatfilePath, string outDir)
        {
            string datfilePath = Path.Combine(outDir, Path.GetFileNameWithoutExtension(sdatfilePath) + ".dat");

            int statuscode = new EDAT().decryptFile(sdatfilePath, datfilePath);

            if (statuscode == 0)
                await RunExtract(datfilePath, outDir);
            else
            {
                LoggerAccessor.LogDebug($"[RunUnBAR] - EDAT decryption failed with status code : {statuscode}, switching to make_npdata...");

                using Process? process = Process.Start(new ProcessStartInfo()
                {
                    FileName = converterPath + "/make_npdata",
                    Arguments = $"-d \"{sdatfilePath}\" \"{datfilePath}\" 0",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WorkingDirectory = converterPath, // Can load various config files.
                    CreateNoWindow = true
                });

                process?.WaitForExit();

                int? ExitCode = process?.ExitCode;

                if (ExitCode != 0)
                    LoggerAccessor.LogError($"[RunUnBAR] - RunDecrypt failed with status code : {ExitCode}");
                else
                    await RunExtract(datfilePath, outDir);
            }
        }

        private static async Task RunExtract(string filePath, string outDir)
        {
            bool isSharc = false;
            bool isLittleEndian = false;
            string options = ToolsImpl.base64DefaultSharcKey;
            byte[]? RawBarData = null;

            if (File.Exists(filePath))
            {
                try
                {
                    RawBarData = await File.ReadAllBytesAsync(filePath);

                    if (DataTypesUtils.FindBytePattern(RawBarData, new byte[] { 0xAD, 0xEF, 0x17, 0xE1, 0x02, 0x00, 0x00, 0x00 }) != -1)
                        isSharc = true;
                    else if (DataTypesUtils.FindBytePattern(RawBarData, new byte[] { 0xE1, 0x17, 0xEF, 0xAD, 0x00, 0x00, 0x00, 0x02 }) != -1)
                    {
                        isSharc = true;
                        isLittleEndian = true;
                    }

                    if (isSharc && RawBarData.Length > 52)
                    {
                        ToolsImpl? toolsImpl = new();

                        try
                        {
                            byte[]? HeaderIV = new byte[16];

                            Buffer.BlockCopy(RawBarData, 8, HeaderIV, 0, HeaderIV.Length);

                            if (HeaderIV != null)
                            {
                                byte[] SharcHeader = new byte[28];

                                Buffer.BlockCopy(RawBarData, 24, SharcHeader, 0, SharcHeader.Length);

                                SharcHeader = AESCTR256EncryptDecrypt.InitiateCTRBuffer(SharcHeader,
                                 Convert.FromBase64String(options), HeaderIV);

                                if (SharcHeader == Array.Empty<byte>())
                                    return; // Sharc Header failed to decrypt.
                                else if (!DataTypesUtils.AreArraysIdentical(new byte[] { SharcHeader[0], SharcHeader[1], SharcHeader[2], SharcHeader[3] }, new byte[4]))
                                {
                                    options = ToolsImpl.base64CDNKey1;

                                    Buffer.BlockCopy(RawBarData, 24, SharcHeader, 0, SharcHeader.Length);

                                    SharcHeader = AESCTR256EncryptDecrypt.InitiateCTRBuffer(SharcHeader,
                                     Convert.FromBase64String(options), HeaderIV);

                                    if (SharcHeader == Array.Empty<byte>())
                                        return; // Sharc Header failed to decrypt.
                                    else if (!DataTypesUtils.AreArraysIdentical(new byte[] { SharcHeader[0], SharcHeader[1], SharcHeader[2], SharcHeader[3] }, new byte[4]))
                                    {
                                        options = ToolsImpl.base64CDNKey2;

                                        Buffer.BlockCopy(RawBarData, 24, SharcHeader, 0, SharcHeader.Length);

                                        SharcHeader = AESCTR256EncryptDecrypt.InitiateCTRBuffer(SharcHeader,
                                         Convert.FromBase64String(options), HeaderIV);

                                        if (SharcHeader == Array.Empty<byte>())
                                            return; // Sharc Header failed to decrypt.
                                        else if (!DataTypesUtils.AreArraysIdentical(new byte[] { SharcHeader[0], SharcHeader[1], SharcHeader[2], SharcHeader[3] }, new byte[4]))
                                            return; // All keys failed to decrypt.
                                    }
                                }

                                byte[] NumOfFiles = new byte[4];

                                if (isLittleEndian == true)
                                    Buffer.BlockCopy(SharcHeader, SharcHeader.Length - 20, NumOfFiles, 0, NumOfFiles.Length);
                                else
                                    Buffer.BlockCopy(EndianUtils.EndianSwap(SharcHeader), SharcHeader.Length - 20, NumOfFiles, 0, NumOfFiles.Length);

                                if (!BitConverter.IsLittleEndian)
                                    Array.Reverse(NumOfFiles);

                                byte[]? SharcTOC = new byte[24 * BitConverter.ToUInt32(NumOfFiles, 0)];

                                Buffer.BlockCopy(RawBarData, 52, SharcTOC, 0, SharcTOC.Length);

                                if (SharcTOC != null)
                                {
                                    byte[] OriginalIV = new byte[HeaderIV.Length];

                                    Buffer.BlockCopy(HeaderIV, 0, OriginalIV, 0, OriginalIV.Length);

                                    toolsImpl.IncrementIVBytes(HeaderIV, 1); // IV so we increment.

                                    SharcTOC = AESCTR256EncryptDecrypt.InitiateCTRBuffer(SharcTOC, Convert.FromBase64String(options), HeaderIV);

                                    if (SharcTOC != Array.Empty<byte>())
                                    {
                                        byte[] SharcData = new byte[RawBarData.Length - (52 + SharcTOC.Length)];

                                        Buffer.BlockCopy(RawBarData, 52 + SharcTOC.Length, SharcData, 0, SharcData.Length);

                                        byte[] FileBytes = Array.Empty<byte>();

                                        if (isLittleEndian)
                                        {
                                            FileBytes = DataTypesUtils.CombineByteArrays(new byte[] { 0xE1, 0x17, 0xEF, 0xAD, 0x00, 0x00, 0x00, 0x02 }, new byte[][]
                                            {
                                                    OriginalIV,
                                                    SharcHeader,
                                                    SharcTOC,
                                                    SharcData
                                            });
                                        }
                                        else
                                        {
                                            FileBytes = DataTypesUtils.CombineByteArrays(new byte[] { 0xAD, 0xEF, 0x17, 0xE1, 0x02, 0x00, 0x00, 0x00 }, new byte[][]
                                            {
                                                    OriginalIV,
                                                    SharcHeader,
                                                    SharcTOC,
                                                    SharcData
                                            });
                                        }

                                        Directory.CreateDirectory(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)));

                                        File.WriteAllBytes(filePath, FileBytes);

                                        LoggerAccessor.LogInfo("Loading SHARC/dat: {0}", filePath);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerAccessor.LogError($"[RunUnBAR] - SHARC Decryption failed! with error - {ex}");
                        }

                        toolsImpl = null;
                    }
                    else
                    {
                        if (DataTypesUtils.FindBytePattern(RawBarData, new byte[] { 0xAD, 0xEF, 0x17, 0xE1 }) != -1)
                        {

                        }
                        else if (DataTypesUtils.FindBytePattern(RawBarData, new byte[] { 0xE1, 0x17, 0xEF, 0xAD }) != -1)
                            isLittleEndian = true;
                        else
                            return; // File not a BAR.

                        Directory.CreateDirectory(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)));

                        LoggerAccessor.LogInfo("Loading BAR/dat: {0}", filePath);
                    }
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[RunUnBAR] - Timestamp creation failed! with error - {ex}");
                }
            }

            if (Directory.Exists(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath))))
            {
                try
                {
                    BARArchive? archive = null;
                    archive = new(filePath, outDir);
                    archive.Load();
                    archive.WriteMap(filePath);
                    File.WriteAllText(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)) + "/timestamp.txt", archive.BARHeader.UserData.ToString("X"));

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
                                        using MemoryStream memoryStream = new(FileData);
                                        ExtractToFileBarVersion1(RawBarData, archive, tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)),
                                            FileTypeAnalyser.Instance.GetRegisteredExtension(FileTypeAnalyser.Instance.Analyse(memoryStream)));
                                        memoryStream.Flush();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LoggerAccessor.LogWarn($"[RunUnBAR] - RunExtract Errored out on file:{tableOfContent.FileName} or failed to scan for extension - {ex}");

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
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[RunUnBAR] - RunExtract Errored out - {ex}");
                }
            }
        }

        private static void ExtractToFileBarVersion1(byte[]? RawBarData, BARArchive archive, HashedFileName FileName, string outDir, string fileType)
        {
            TOCEntry? tableOfContent = archive.TableOfContents[FileName];
            string path = string.Empty;
            if (string.IsNullOrEmpty(tableOfContent.Path))
                path = string.Format("{0}{1}{2:X8}{3}", outDir, Path.DirectorySeparatorChar, FileName.Value, fileType);
            else
                path = string.Format("{0}{1}{2}", outDir, Path.DirectorySeparatorChar, tableOfContent.Path.Replace('/', Path.DirectorySeparatorChar));
            string? outdirectory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(outdirectory))
            {
                Directory.CreateDirectory(outdirectory);
                using (FileStream fileStream = File.Open(path, (FileMode)2))
                {
                    byte[] data = tableOfContent.GetData(archive.GetHeader().Flags);
                    if (tableOfContent.Compression == CompressionMethod.Encrypted && 
                        ((data[0] == 0x00 && data[1] == 0x00 && data[2] == 0x00 && data[3] == 0x01) || (data[0] == 0x01 && data[1] == 0x00 && data[2] == 0x00 && data[3] == 0x00)))
                    {
                        int dataStart = FindDataPositionInBinary(RawBarData, data);

                        if (dataStart != -1)
                        {
                            uint compressedSize = tableOfContent.CompressedSize;
                            uint fileSize = tableOfContent.Size;
                            int userData = archive.BARHeader.UserData;
                            byte[] EncryptedSignatureHeader = new byte[24];
                            BlowfishCTREncryptDecrypt? blowfish = new();
                            ToolsImpl? toolsImpl = new();
#if DEBUG
                            LoggerAccessor.LogInfo("[RunUnBAR] - Encrypted Content Detected!, Running Decryption.");
                            LoggerAccessor.LogInfo($"CompressedSize - {compressedSize}");
                            LoggerAccessor.LogInfo($"Size - {fileSize}");
                            LoggerAccessor.LogInfo($"dataStart - 0x{dataStart:X}");
                            LoggerAccessor.LogInfo($"UserData - 0x{userData:X}");
#endif
                            byte[] SignatureIV = BitConverter.GetBytes(toolsImpl.BuildSignatureIv((int)fileSize, (int)compressedSize, dataStart, userData));

                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(SignatureIV);

                            // Copy the first 24 bytes from the source array to the destination array
                            Buffer.BlockCopy(data, 4, EncryptedSignatureHeader, 0, EncryptedSignatureHeader.Length);

                            byte[]? DecryptedSignatureHeader = blowfish.EncryptionProxyInit(EncryptedSignatureHeader, SignatureIV);

                            if (DecryptedSignatureHeader != null)
                            {
                                string SignatureHeaderHexString = DataTypesUtils.ByteArrayToHexString(DecryptedSignatureHeader);
#if DEBUG
                                LoggerAccessor.LogInfo($"SignatureHeader - {SignatureHeaderHexString}");
#endif
                                // Create a new byte array to store the remaining content
                                byte[]? FileBytes = new byte[data.Length - 28];

                                // Copy the content after the first 28 bytes to the new array
                                Array.Copy(data, 28, FileBytes, 0, FileBytes.Length);

                                StringBuilder sb = new();

                                foreach (byte b in SHA1.HashData(FileBytes))
                                {
                                    sb.Append(b.ToString("x2")); // Convert each byte to a hexadecimal string
                                }

                                string SHA1HexString = sb.ToString();

                                if (string.Equals(SHA1HexString, SignatureHeaderHexString[..^8], StringComparison.CurrentCultureIgnoreCase)) // We strip the original file Compression size.
                                {
                                    if (tableOfContent.Size == 0) // The original Encryption Proxy seemed to only check for "lua" or "scene" file types, regardless if empty or not.
                                    {
                                        fileStream.Write(FileBytes, 0, FileBytes.Length);
                                        fileStream.Close();
                                    }
                                    else
                                    {
                                        toolsImpl.IncrementIVBytes(SignatureIV, 3);

                                        FileBytes = blowfish.InitiateCTRBuffer(FileBytes, SignatureIV);

                                        if (FileBytes != null)
                                        {
                                            try
                                            {
                                                FileBytes = toolsImpl.ComponentAceEdgeZlibDecompress(FileBytes);
                                            }
                                            catch
                                            {
                                                // Explanation, some files requires ICSharp handling for decompression, this is an expected behaviour.

                                                LoggerAccessor.LogDebug($"[RunUnBar] - ComponentAce failed to decompress file, switching to ICSharp engine...");

                                                try
                                                {
                                                    FileBytes = toolsImpl.ICSharpEdgeZlibDecompress(FileBytes);
                                                }
                                                catch (Exception ex)
                                                {
                                                    LoggerAccessor.LogError($"[RunUnBar] - Errored out when processing Encryption Proxy encrypted content - {ex}");

                                                    FileBytes = data;
                                                }
                                            }

                                            fileStream.Write(FileBytes, 0, FileBytes.Length);
                                            fileStream.Close();
                                        }
                                        else
                                        {
                                            LoggerAccessor.LogError($"[RunUnBAR] - Encrypted file failed to decrypt, Writing original data.");
                                            fileStream.Write(data, 0, data.Length);
                                            fileStream.Close();
                                        }
                                    }
                                }
                                else
                                {
                                    LoggerAccessor.LogError($"[RunUnBAR] - Encrypted file (SHA1 - {SHA1HexString}) has been tempered with! (Reference SHA1 - {SignatureHeaderHexString[..^8]}), Aborting decryption.");
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

        private static void ExtractToFileBarVersion2(byte[] Key, BARArchive archive, HashedFileName FileName, string outDir)
        {
            TOCEntry? tableOfContent = archive.TableOfContents[FileName];
            ToolsImpl? toolsImpl = new();
            string path = string.Empty;
            if (!string.IsNullOrEmpty(tableOfContent.Path))
                path = string.Format("{0}{1}{2}", outDir, Path.DirectorySeparatorChar, tableOfContent.Path.Replace('/', Path.DirectorySeparatorChar));
            byte[] data = tableOfContent.GetData(archive.GetHeader().Flags);
            if (tableOfContent.Compression == CompressionMethod.Encrypted)
            {
#if DEBUG
                LoggerAccessor.LogInfo("[RunUnBAR] - Encrypted Content Detected!, Running Decryption.");
                LoggerAccessor.LogInfo($"Key - {DataTypesUtils.ByteArrayToHexString(Key)}");
                LoggerAccessor.LogInfo($"IV - {DataTypesUtils.ByteArrayToHexString(tableOfContent.IV)}");
#endif

                byte[]? FileBytes = toolsImpl.ProcessLibsecureXTEABlocks(data, Key, tableOfContent.IV);

                try
                {
                    FileBytes = toolsImpl.ComponentAceEdgeZlibDecompress(FileBytes);
                }
                catch
                {
                    // Explanation, some files requires ICSharp handling for decompression, this is an expected behaviour.

                    LoggerAccessor.LogDebug($"[RunUnBar] - ComponentAce failed to decompress file, switching to ICSharp engine...");

                    try
                    {
                        FileBytes = toolsImpl.ICSharpEdgeZlibDecompress(FileBytes);
                    }
                    catch (Exception ex)
                    {
                        LoggerAccessor.LogError($"[RunUnBar] - Errored out when processing XTEA Proxy encrypted content - {ex}");

                        FileBytes = data;
                    }
                }

                using MemoryStream memoryStream = new(FileBytes);
                string registeredExtension = string.Empty;

                try
                {
                    registeredExtension = FileTypeAnalyser.Instance.GetRegisteredExtension(FileTypeAnalyser.Instance.Analyse(memoryStream));
                }
                catch
                {
                    registeredExtension = ".unknown";
                }

                if (path == string.Empty)
                    path = string.Format("{0}{1}{2:X8}{3}", outDir, Path.DirectorySeparatorChar, FileName.Value, registeredExtension);

                string? outdirectory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(outdirectory))
                {
                    Directory.CreateDirectory(outdirectory);

                    using FileStream fileStream = File.Open(path, (FileMode)2);
                    fileStream.Write(FileBytes, 0, FileBytes.Length);
                    fileStream.Close();
                }

                memoryStream.Flush();
            }
            else
            {
                using MemoryStream memoryStream = new(data);
                string registeredExtension = string.Empty;

                try
                {
                    registeredExtension = FileTypeAnalyser.Instance.GetRegisteredExtension(FileTypeAnalyser.Instance.Analyse(memoryStream));
                }
                catch
                {
                    registeredExtension = ".unknown";
                }

                if (path == string.Empty)
                    path = string.Format("{0}{1}{2:X8}{3}", outDir, Path.DirectorySeparatorChar, FileName.Value, registeredExtension);

                string? outdirectory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(outdirectory))
                {
                    Directory.CreateDirectory(outdirectory);

                    using FileStream fileStream = File.Open(path, (FileMode)2);
                    fileStream.Write(data, 0, data.Length);
                    fileStream.Close();
                }

                memoryStream.Flush();
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


        /// <summary>
        /// Finds a matching byte array within an other byte array.
        /// <para>Trouve un tableau de bytes correspondant dans un autre tableau de bytes.</para>
        /// </summary>
        /// <param name="data1">The data to search for.</param>
        /// <param name="data2">The data to search into for the data1.</param>
        /// <returns>A int (-1 if not found).</returns>
        private static int FindDataPositionInBinary(byte[]? data1, byte[] data2)
        {
            if (data1 == null)
                return -1;

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
