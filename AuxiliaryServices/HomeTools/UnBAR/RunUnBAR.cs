using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using CompressionLibrary.Edge;
using NetworkLibrary.Extension;
using HomeTools.BARFramework;
using HomeTools.Crypto;
using HomeTools.PS3_Creator;
using EndianTools;
using CustomLogger;

namespace HomeTools.UnBAR
{
    public static class RunUnBAR
    {
        public static async Task Run(string converterPath, string filePath, string outputpath, bool edat, ushort cdnMode)
        {
            if (edat)
                await RunDecrypt(converterPath, filePath, outputpath, cdnMode);
            else
                await RunExtract(filePath, outputpath, cdnMode);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect(); // We have no choice and it's not possible to remove them, HomeTools create a BUNCH of necessary objects.
        }

        public static void RunEncrypt(string filePath, string sdatfilePath)
        {
            try
            {
                int ExitCode = new EDAT().encryptFile(filePath, sdatfilePath, new byte[16], null, new byte[48], ConversionUtils.getByteArray("0C"), ConversionUtils.getByteArray("00"), ConversionUtils.getByteArray("03"));

                if (ExitCode != 0)
                    LoggerAccessor.LogError($"[RunUnBAR] - RunEncrypt failed with status code : {ExitCode}");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RunUnBAR] - RunEncrypt failed with assertion : {ex}");
            }
        }

        private static async Task RunDecrypt(string converterPath, string sdatfilePath, string outDir, ushort cdnMode)
        {
            string datfilePath = Path.Combine(outDir, Path.GetFileNameWithoutExtension(sdatfilePath) + ".dat");

            try
            {
                int ExitCode = new EDAT().decryptFile(sdatfilePath, datfilePath, new byte[16], null);

                if (ExitCode != 0)
                    LoggerAccessor.LogError($"[RunUnBAR] - RunDecrypt failed with status code : {ExitCode}");
                else if (ExitCode == sbyte.MinValue)
                {
                    string makeNpExePath = converterPath + "/make_npdata/" + (!NetworkLibrary.Extension.Windows.Win32API.IsWindows ? "make_npdata_win32.exe" : "make_npdata");

                    if (File.Exists(makeNpExePath))
                    {
                        using (Process process = Process.Start(new ProcessStartInfo()
                        {
                            FileName = makeNpExePath,
                            Arguments = $"-d \"{sdatfilePath}\" \"{datfilePath}\" 0",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            WorkingDirectory = converterPath, // Can load various config files.
                            CreateNoWindow = true
                        }))
                        {
                            process.WaitForExit();

                            ExitCode = process.ExitCode;

                            if (ExitCode != 0)
                                LoggerAccessor.LogError($"[RunUnBAR] - RunDecrypt failed with makenpdata process status code : {ExitCode}");
                            else
                                await RunExtract(datfilePath, outDir, cdnMode);
                        }
                    }
                    else
                        LoggerAccessor.LogError($"[RunUnBAR] - RunDecrypt dectected LZ compressed data, but no makenpdata executable were found at path: {makeNpExePath}");
                }
                else
                    await RunExtract(datfilePath, outDir, cdnMode);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RunUnBAR] - RunDecrypt failed with assertion : {ex}");
            }
        }

        private static Task RunExtract(string filePath, string outDir, ushort cdnMode)
        {
            bool isSharc = false;
            bool isLittleEndian = false;
            string options = ToolsImplementation.base64CDNKey2;
            byte[] RawBarData = null;

            if (File.Exists(filePath))
            {
                string barDirectoryPath = Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath));

                try
                {
                    RawBarData = File.ReadAllBytes(filePath);

                    if (RawBarData.Length < 12)
                        return Task.CompletedTask; // File not a BAR.
                    else
                    {
                        if (RawBarData[0] == 0xAD && RawBarData[1] == 0xEF && RawBarData[2] == 0x17 && RawBarData[3] == 0xE1)
                        {

                        }
                        else if (RawBarData[0] == 0xE1 && RawBarData[1] == 0x17 && RawBarData[2] == 0xEF && RawBarData[3] == 0xAD)
                            isLittleEndian = true;
                        else
                            return Task.CompletedTask; // File not a BAR.

                        switch (isLittleEndian)
                        {
                            case true:
                                if (RawBarData[4] == 0x00 && RawBarData[5] == 0x00 && RawBarData[6] == 0x00 && RawBarData[7] == 0x02)
                                    isSharc = true;
                                break;
                            default:
                                if (RawBarData[4] == 0x02 && RawBarData[5] == 0x00 && RawBarData[6] == 0x00 && RawBarData[7] == 0x00)
                                    isSharc = true;
                                break;
                        }
                    }

                    if (isSharc && RawBarData.Length > 52)
                    {
                        try
                        {
                            byte[] HeaderIV = new byte[16];

                            Buffer.BlockCopy(RawBarData, 8, HeaderIV, 0, HeaderIV.Length);

                            if (HeaderIV != null)
                            {
                                byte[] EmptyArray = new byte[4];
                                byte[] SharcHeader = new byte[28];

                                Buffer.BlockCopy(RawBarData, 24, SharcHeader, 0, SharcHeader.Length);

                                SharcHeader = LIBSECURE.InitiateAESBuffer(SharcHeader,
                                 Convert.FromBase64String(options), HeaderIV, "CTR");

                                if (SharcHeader == null)
                                    return Task.CompletedTask; // Sharc Header failed to decrypt.
                                else if (!(new byte[] { SharcHeader[0], SharcHeader[1], SharcHeader[2], SharcHeader[3] }).EqualsTo(EmptyArray))
                                {
                                    options = ToolsImplementation.base64CDNKey1;

                                    Buffer.BlockCopy(RawBarData, 24, SharcHeader, 0, SharcHeader.Length);

                                    SharcHeader = LIBSECURE.InitiateAESBuffer(SharcHeader,
                                     Convert.FromBase64String(options), HeaderIV, "CTR");

                                    if (SharcHeader == null)
                                        return Task.CompletedTask; // Sharc Header failed to decrypt.
                                    else if (!(new byte[] { SharcHeader[0], SharcHeader[1], SharcHeader[2], SharcHeader[3] }).EqualsTo(EmptyArray))
                                    {
                                        options = ToolsImplementation.base64DefaultSharcKey;

                                        Buffer.BlockCopy(RawBarData, 24, SharcHeader, 0, SharcHeader.Length);

                                        SharcHeader = LIBSECURE.InitiateAESBuffer(SharcHeader,
                                         Convert.FromBase64String(options), HeaderIV, "CTR");

                                        if (SharcHeader == null)
                                            return Task.CompletedTask; // Sharc Header failed to decrypt.
                                        else if (!(new byte[] { SharcHeader[0], SharcHeader[1], SharcHeader[2], SharcHeader[3] }).EqualsTo(EmptyArray))
                                            return Task.CompletedTask; // All keys failed to decrypt.
                                    }
                                }

                                byte[] NumOfFiles = new byte[4];

                                if (isLittleEndian == true)
                                    Buffer.BlockCopy(SharcHeader, SharcHeader.Length - 20, NumOfFiles, 0, NumOfFiles.Length);
                                else
                                    Buffer.BlockCopy(EndianUtils.EndianSwap(SharcHeader), SharcHeader.Length - 20, NumOfFiles, 0, NumOfFiles.Length);

                                if (!BitConverter.IsLittleEndian)
                                    Array.Reverse(NumOfFiles);

                                byte[] SharcTOC = new byte[24 * BitConverter.ToUInt32(NumOfFiles, 0)];

                                Buffer.BlockCopy(RawBarData, 52, SharcTOC, 0, SharcTOC.Length);

                                if (SharcTOC != null)
                                {
                                    byte[] OriginalIV = new byte[HeaderIV.Length];

                                    Buffer.BlockCopy(HeaderIV, 0, OriginalIV, 0, OriginalIV.Length);

                                    ToolsImplementation.IncrementIVBytes(HeaderIV, 1); // IV so we increment.

                                    SharcTOC = LIBSECURE.InitiateAESBuffer(SharcTOC, Convert.FromBase64String(options), HeaderIV, "CTR");

                                    if (SharcTOC != null)
                                    {
                                        byte[] SharcData = new byte[RawBarData.Length - (52 + SharcTOC.Length)];

                                        Buffer.BlockCopy(RawBarData, 52 + SharcTOC.Length, SharcData, 0, SharcData.Length);

                                        byte[] FileBytes = Array.Empty<byte>();

                                        if (isLittleEndian)
                                        {
                                            FileBytes = ByteUtils.CombineByteArrays(new byte[] { 0xE1, 0x17, 0xEF, 0xAD, 0x00, 0x00, 0x00, 0x02 }, new byte[][]
                                            {
                                                    OriginalIV,
                                                    SharcHeader,
                                                    SharcTOC,
                                                    SharcData
                                            });
                                        }
                                        else
                                        {
                                            FileBytes = ByteUtils.CombineByteArrays(new byte[] { 0xAD, 0xEF, 0x17, 0xE1, 0x02, 0x00, 0x00, 0x00 }, new byte[][]
                                            {
                                                    OriginalIV,
                                                    SharcHeader,
                                                    SharcTOC,
                                                    SharcData
                                            });
                                        }

                                        Directory.CreateDirectory(barDirectoryPath);

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
                    }
                    else
                    {
                        Directory.CreateDirectory(barDirectoryPath);

                        LoggerAccessor.LogInfo("Loading BAR/dat: {0}", filePath);
                    }
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[RunUnBAR] - Initial archive loading failed with error - {ex}");
                }

                if (Directory.Exists(barDirectoryPath))
                {
                    try
                    {
                        BARArchive archive = new BARArchive(null, filePath, outDir);
                        archive.Load();
                        //archive.WriteMap(filePath);
                        File.WriteAllText(barDirectoryPath + "/timestamp.txt", archive.BARHeader.UserData.ToString("X"));
#if NETCOREAPP || NETSTANDARD1_0_OR_GREATER || NET40_OR_GREATER
                        // Process Environment.ProcessorCount patherns at a time, removing the limit is not tolerable as CPU usage can go high.
                        Parallel.ForEach(archive.TableOfContents.Cast<TOCEntry>(), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, tableOfContent =>
                        {
                            byte[] FileData = tableOfContent.GetData(archive.GetHeader().Flags);

                            try
                            {
                                if (archive.GetHeader().Version == 512)
                                    ExtractToFileBarVersion2(archive.GetHeader().Key, FileData, archive, tableOfContent.FileName, barDirectoryPath);
                                else
                                    ExtractToFileBarVersion1(RawBarData, FileData, archive, tableOfContent.FileName, barDirectoryPath, cdnMode);
                            }
                            catch (Exception ex)
                            {
                                LoggerAccessor.LogWarn($"[RunUnBAR] - RunExtract Errored out on file:{tableOfContent.FileName} (Exception: {ex})");

                                if (archive.GetHeader().Version == 512)
                                    ExtractToFileBarVersion2(archive.GetHeader().Key, FileData, archive, tableOfContent.FileName, barDirectoryPath);
                                else
                                    ExtractToFileBarVersion1(RawBarData, FileData, archive, tableOfContent.FileName, barDirectoryPath, cdnMode);
                            }
                        });
#else
                        foreach (TOCEntry tableOfContent in archive.TableOfContents)
                        {
                            byte[] FileData = tableOfContent.GetData(archive.GetHeader().Flags);

                            try
                            {
                                if (archive.GetHeader().Version == 512)
                                    ExtractToFileBarVersion2(archive.GetHeader().Key, FileData, archive, tableOfContent.FileName, barDirectoryPath);
                                else
                                    ExtractToFileBarVersion1(RawBarData, FileData, archive, tableOfContent.FileName, barDirectoryPath, cdnMode);
                            }
                            catch (Exception ex)
                            {
                                LoggerAccessor.LogWarn($"[RunUnBAR] - RunExtract Errored out on file:{tableOfContent.FileName} (Exception: {ex})");

                                if (archive.GetHeader().Version == 512)
                                    ExtractToFileBarVersion2(archive.GetHeader().Key, FileData, archive, tableOfContent.FileName, barDirectoryPath);
                                else
                                    ExtractToFileBarVersion1(RawBarData, FileData, archive, tableOfContent.FileName, barDirectoryPath, cdnMode);
                            }
                        }
#endif
                        if (File.Exists(filePath + ".map"))
                            File.Move(filePath + ".map", barDirectoryPath + $"/{Path.GetFileName(filePath)}.map");
                        else if (filePath.Length > 4 && File.Exists(filePath.Substring(0, filePath.Length - 4) + ".sharc.map"))
                            File.Move(filePath.Substring(0, filePath.Length - 4) + ".sharc.map", barDirectoryPath + $"/{Path.GetFileName(filePath)}.map");
                        else if (filePath.Length > 4 && File.Exists(filePath.Substring(0, filePath.Length - 4) + ".bar.map"))
                            File.Move(filePath.Substring(0, filePath.Length - 4) + ".bar.map", barDirectoryPath + $"/{Path.GetFileName(filePath)}.map");
                    }
                    catch (Exception ex)
                    {
                        LoggerAccessor.LogError($"[RunUnBAR] - RunExtract Errored out - {ex}");
                    }
                }
            }

            return Task.CompletedTask;
        }

        private static async void ExtractToFileBarVersion1(byte[] RawBarData, byte[] data, BARArchive archive, HashedFileName FileName, string outDir, int cdnMode)
        {
            TOCEntry tableOfContent = archive.TableOfContents[FileName];
            string path = null;
            if (tableOfContent.Compression == CompressionMethod.Encrypted &&
                data.Length > 4 && ((data[0] == 0x00 && data[1] == 0x00 && data[2] == 0x00 && data[3] == 0x01) || (data[0] == 0x01 && data[1] == 0x00 && data[2] == 0x00 && data[3] == 0x00)))
            {
                int dataStart = FindDataPositionInBinary(RawBarData, data);

                if (dataStart != -1)
                {
                    uint compressedSize = tableOfContent.CompressedSize;
                    uint fileSize = tableOfContent.Size;
                    int userData = archive.BARHeader.UserData;
                    byte[] EncryptedSignatureHeader = new byte[24];
#if DEBUG
                    LoggerAccessor.LogInfo("[RunUnBAR] - Encrypted Content Detected!, Running Decryption.");
                    LoggerAccessor.LogInfo($"CompressedSize - {compressedSize}");
                    LoggerAccessor.LogInfo($"Size - {fileSize}");
                    LoggerAccessor.LogInfo($"dataStart - 0x{dataStart:X}");
                    LoggerAccessor.LogInfo($"UserData - 0x{userData:X}");
#endif
                    byte[] SignatureIV = BitConverter.GetBytes(ToolsImplementation.BuildSignatureIv((int)fileSize, (int)compressedSize, dataStart, userData));

                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(SignatureIV);

                    // Copy the first 24 bytes from the source array to the destination array
                    Buffer.BlockCopy(data, 4, EncryptedSignatureHeader, 0, EncryptedSignatureHeader.Length);

                    byte[] DecryptedSignatureHeader;

                    switch (cdnMode)
                    {
                        case 2:
                            DecryptedSignatureHeader = LIBSECURE.InitiateBlowfishBuffer(EncryptedSignatureHeader, ToolsImplementation.HDKSignatureKey, SignatureIV, "CTR");
                            break;
                        case 1:
                            DecryptedSignatureHeader = LIBSECURE.InitiateBlowfishBuffer(EncryptedSignatureHeader, ToolsImplementation.BetaSignatureKey, SignatureIV, "CTR");
                            break;
                        default:
                            DecryptedSignatureHeader = LIBSECURE.InitiateBlowfishBuffer(EncryptedSignatureHeader, ToolsImplementation.SignatureKey, SignatureIV, "CTR");
                            break;
                    }

                    if (DecryptedSignatureHeader != null)
                    {
                        string SignatureHeaderHexString = DecryptedSignatureHeader.ToHexString();

                        // Create a new byte array to store the remaining content
                        byte[] FileBytes = new byte[data.Length - 28];

                        // Copy the content after the first 28 bytes to the new array
                        Array.Copy(data, 28, FileBytes, 0, FileBytes.Length);

                        string SHA1HexString = NetHasher.DotNetHasher.ComputeSHA1String(FileBytes);

                        if (string.Equals(SHA1HexString, SignatureHeaderHexString.Substring(0, SignatureHeaderHexString.Length - 8))) // We strip the original file Compression size.
                        {
                            if (tableOfContent.Size == 0) // The original Encryption Proxy seemed to only check for "lua" or "scene" file types, regardless if empty or not.
                            {
                                path = string.Format("{0}{1}{2:X8}{3}", outDir, Path.DirectorySeparatorChar, FileName.Value, ".unknown").ToUpper();

                                string outdirectory = Path.GetDirectoryName(path);
                                if (!string.IsNullOrEmpty(outdirectory))
                                {
                                    Directory.CreateDirectory(outdirectory);

                                    using (FileStream fileStream = File.Open(path, (FileMode)2))
                                    {
                                        fileStream.Write(FileBytes, 0, FileBytes.Length);
                                        fileStream.Close();
                                    }
                                }
#if DEBUG
                                LoggerAccessor.LogInfo("Extracted file {0}", new object[1]
                                {
                                    Path.GetFileName(path)
                                });
#endif
                                tableOfContent = null;

                                return;
                            }
                            else
                            {
                                ToolsImplementation.IncrementIVBytes(SignatureIV, 3);

                                switch (cdnMode)
                                {
                                    case 2:
                                        FileBytes = LIBSECURE.InitiateBlowfishBuffer(FileBytes, ToolsImplementation.HDKBlowfishKey, SignatureIV, "CTR");
                                        break;
                                    case 1:
                                        FileBytes = LIBSECURE.InitiateBlowfishBuffer(FileBytes, ToolsImplementation.BetaBlowfishKey, SignatureIV, "CTR");
                                        break;
                                    default:
                                        FileBytes = LIBSECURE.InitiateBlowfishBuffer(FileBytes, ToolsImplementation.BlowfishKey, SignatureIV, "CTR");
                                        break;
                                }

                                if (FileBytes != null)
                                {
                                    try
                                    {
                                        FileBytes = await Zlib.EdgeZlibDecompress(FileBytes).ConfigureAwait(false);
                                    }
                                    catch
                                    {
                                        // Explanation, some files requires ICSharp handling for decompression, this is an expected behaviour.

                                        LoggerAccessor.LogDebug($"[RunUnBar] - ComponentAce failed to decompress file, switching to ICSharp engine...");

                                        try
                                        {
                                            FileBytes = await Zlib.EdgeZlibDecompress(FileBytes, true).ConfigureAwait(false);
                                        }
                                        catch (Exception ex)
                                        {
                                            LoggerAccessor.LogError($"[RunUnBar] - Errored out when processing Encryption Proxy encrypted content - {ex}");

                                            FileBytes = data;
                                        }
                                    }

                                    using (MemoryStream memoryStream = new MemoryStream(FileBytes))
                                    {
                                        string registeredExtension = string.Empty;

                                        try
                                        {
                                            registeredExtension = FileTypeAnalyser.Instance.GetRegisteredExtension(FileTypeAnalyser.Instance.Analyse(memoryStream));
                                        }
                                        catch
                                        {
                                            registeredExtension = ".unknown";
                                        }

                                        path = string.Format("{0}{1}{2:X8}{3}", outDir, Path.DirectorySeparatorChar, FileName.Value, registeredExtension).ToUpper();

                                        string outdirectory = Path.GetDirectoryName(path);
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
#if DEBUG
                                    LoggerAccessor.LogInfo("Extracted file {0}", new object[1]
                                    {
                                        Path.GetFileName(path)
                                    });
#endif
                                    tableOfContent = null;

                                    return;
                                }
                                else
                                    LoggerAccessor.LogError($"[RunUnBAR] - Encrypted file failed to decrypt, Writing original data.");
                            }
                        }
                        else
                            LoggerAccessor.LogError($"[RunUnBAR] - Encrypted file (SHA1 - {SHA1HexString}) has been tempered with! (Reference SHA1 - {SignatureHeaderHexString.Substring(0, SignatureHeaderHexString.Length - 8)}), Aborting decryption.");
                    }
                    else
                        LoggerAccessor.LogError("[RunUnBAR] - Encrypted data SignatureHeader Decryption has failed.");
                }
                else
                    LoggerAccessor.LogError("[RunUnBAR] - Encrypted data not found in BAR or false positive! Decryption has failed.");
            }

            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                string registeredExtension = string.Empty;

                try
                {
                    registeredExtension = FileTypeAnalyser.Instance.GetRegisteredExtension(FileTypeAnalyser.Instance.Analyse(memoryStream));
                }
                catch
                {
                    registeredExtension = ".unknown";
                }

                path = string.Format("{0}{1}{2:X8}{3}", outDir, Path.DirectorySeparatorChar, FileName.Value, registeredExtension).ToUpper();

                string outdirectory = Path.GetDirectoryName(path);
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
#if DEBUG
            LoggerAccessor.LogInfo("Extracted file {0}", new object[1]
            {
                    Path.GetFileName(path)
            });
#endif
            tableOfContent = null;
        }

        private static async void ExtractToFileBarVersion2(byte[] Key, byte[] data, BARArchive archive, HashedFileName FileName, string outDir)
        {
            TOCEntry tableOfContent = archive.TableOfContents[FileName];
            string path = null;
            if (tableOfContent.Compression == CompressionMethod.Encrypted)
            {
#if DEBUG
                LoggerAccessor.LogInfo("[RunUnBAR] - Encrypted Content Detected!, Running Decryption.");
                LoggerAccessor.LogInfo($"Key - {Key.ToHexString()}");
                LoggerAccessor.LogInfo($"IV - {tableOfContent.IV.ToHexString()}");
#endif

                byte[] FileBytes = await ToolsImplementation.ProcessXTEAProxyAsync(data, Key, tableOfContent.IV).ConfigureAwait(false);

                try
                {
                    FileBytes = await Zlib.EdgeZlibDecompress(FileBytes).ConfigureAwait(false);
                }
                catch
                {
                    // Explanation, some files requires ICSharp handling for decompression, this is an expected behaviour.

                    LoggerAccessor.LogDebug($"[RunUnBar] - ComponentAce failed to decompress file, switching to ICSharp engine...");

                    try
                    {
                        FileBytes = await Zlib.EdgeZlibDecompress(FileBytes, true).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        LoggerAccessor.LogError($"[RunUnBar] - Errored out when processing XTEA Proxy encrypted content - {ex}");

                        FileBytes = data;
                    }
                }

                using (MemoryStream memoryStream = new MemoryStream(FileBytes))
                {
                    string registeredExtension = string.Empty;

                    try
                    {
                        registeredExtension = FileTypeAnalyser.Instance.GetRegisteredExtension(FileTypeAnalyser.Instance.Analyse(memoryStream));
                    }
                    catch
                    {
                        registeredExtension = ".unknown";
                    }

                    path = string.Format("{0}{1}{2:X8}{3}", outDir, Path.DirectorySeparatorChar, FileName.Value, registeredExtension).ToUpper();

                    string outdirectory = Path.GetDirectoryName(path);
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
                using (MemoryStream memoryStream = new MemoryStream(data))
                {
                    string registeredExtension = string.Empty;

                    try
                    {
                        registeredExtension = FileTypeAnalyser.Instance.GetRegisteredExtension(FileTypeAnalyser.Instance.Analyse(memoryStream));
                    }
                    catch
                    {
                        registeredExtension = ".unknown";
                    }

                    path = string.Format("{0}{1}{2:X8}{3}", outDir, Path.DirectorySeparatorChar, FileName.Value, registeredExtension).ToUpper();

                    string outdirectory = Path.GetDirectoryName(path);
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
        }


        /// <summary>
        /// Finds a matching byte array within an other byte array.
        /// <para>Trouve un tableau de bytes correspondant dans un autre tableau de bytes.</para>
        /// </summary>
        /// <param name="data1">The data to search for.</param>
        /// <param name="data2">The data to search into for the data1.</param>
        /// <returns>A int (-1 if not found).</returns>
        private static int FindDataPositionInBinary(byte[] data1, byte[] data2)
        {
            if (data1 == null || data2 == null || data1.Length < data2.Length)
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
