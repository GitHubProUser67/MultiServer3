using HomeTools.AFS;
using HomeTools.BARFramework;
using HomeTools.ChannelID;
using HomeTools.Crypto;
using HomeTools.UnBAR;
using BackendProject.MiscUtils;
using WebUtils.CDS;
using CustomLogger;
using HttpMultipartParser;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace WebUtils
{
    public class HomeToolsInterface
    {
        public static (byte[]?, string)? MakeBarSdat(string converterPath, Stream? PostData, string? ContentType)
        {
            (byte[]?, string)? output = null;
            List<(byte[]?, string)?> TasksResult = new();

            if (PostData != null && !string.IsNullOrEmpty(ContentType))
            {
                string maindir = Directory.GetCurrentDirectory() + $"/static/HomeToolsCache/MakeBarSdat_cache/{GenerateDynamicCacheGuid(VariousUtils.GetCurrentDateTime())}";
                Directory.CreateDirectory(maindir);
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);
                if (!string.IsNullOrEmpty(boundary))
                {
                    using MemoryStream ms = new();
                    PostData.CopyTo(ms);
                    ms.Position = 0;
                    int i = 0;
                    string filename = string.Empty;
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    string mode = data.GetParameterValue("mode");
                    string TimeStamp = data.GetParameterValue("TimeStamp");
                    string options = data.GetParameterValue("options");
                    string leanzlib = string.Empty;
                    string encrypt = string.Empty;
                    string version2 = string.Empty;
                    string bigendian = string.Empty;
                    try
                    {
                        leanzlib = data.GetParameterValue("leanzlib");
                    }
                    catch (Exception)
                    {
                        // Not Important
                    }
                    try
                    {
                        encrypt = data.GetParameterValue("encrypt");
                    }
                    catch (Exception)
                    {
                        // Not Important
                    }
                    try
                    {
                        version2 = data.GetParameterValue("version2");
                    }
                    catch (Exception)
                    {
                        // Not Important
                    }
                    try
                    {
                        bigendian = data.GetParameterValue("bigendian");
                    }
                    catch (Exception)
                    {
                        // Not Important
                    }
                    options = options switch
                    {
                        "cdn1" => ToolsImpl.base64CDNKey1,
                        "cdn2" => ToolsImpl.base64CDNKey2,
                        _ => ToolsImpl.base64DefaultSharcKey,
                    };
                    foreach (FilePart? multipartfile in data.Files)
                    {
                        using Stream filedata = multipartfile.Data;
                        filedata.Position = 0;

                        // Find the number of bytes in the stream
                        int contentLength = (int)filedata.Length;

                        // Create a byte array
                        byte[] buffer = new byte[contentLength];

                        // Read the contents of the memory stream into the byte array
                        filedata.Read(buffer, 0, contentLength);

                        filename = multipartfile.FileName;

                        string guid = GenerateDynamicCacheGuid(filename);

                        string tempdir = $"{maindir}/{guid}";

                        string unzipdir = tempdir + $"/unzip";

                        string rebardir = tempdir + $"/archive";

                        string zipfile = tempdir + $"/{filename}";

                        if (Directory.Exists(tempdir))
                            Directory.Delete(tempdir, true);

                        Directory.CreateDirectory(rebardir);

                        File.WriteAllBytes(zipfile, buffer);

                        UncompressFile(zipfile, unzipdir);

                        filename = filename[..^4].ToUpper();

                        IEnumerable<string> enumerable = Directory.EnumerateFiles(unzipdir, "*.*", SearchOption.AllDirectories);
                        BARArchive? bararchive = null;
                        if (version2 == "on")
                        {
                            if (bigendian == "on")
                                bararchive = new BARArchive(string.Format("{0}/{1}.SHARC", rebardir, filename), unzipdir, Convert.ToInt32(TimeStamp, 16), true, true, options);
                            else
                                bararchive = new BARArchive(string.Format("{0}/{1}.SHARC", rebardir, filename), unzipdir, Convert.ToInt32(TimeStamp, 16), true, false, options);
                        }
                        else
                        {
                            if (encrypt == "on")
                            {
                                if (bigendian == "on")
                                    bararchive = new BARArchive(string.Format("{0}/{1}.BAR", rebardir, filename), unzipdir, Convert.ToInt32(TimeStamp, 16), true, true);
                                else
                                    bararchive = new BARArchive(string.Format("{0}/{1}.BAR", rebardir, filename), unzipdir, Convert.ToInt32(TimeStamp, 16), true);
                            }
                            else
                            {
                                if (bigendian == "on")
                                    bararchive = new BARArchive(string.Format("{0}/{1}.BAR", rebardir, filename), unzipdir, Convert.ToInt32(TimeStamp, 16), false, true);
                                else
                                    bararchive = new BARArchive(string.Format("{0}/{1}.BAR", rebardir, filename), unzipdir, Convert.ToInt32(TimeStamp, 16));
                            }
                            if (leanzlib == "on")
                            {
                                bararchive.BARHeader.Flags = ArchiveFlags.Bar_Flag_LeanZLib;
                                bararchive.DefaultCompression = CompressionMethod.ZLib;
                            }
                            else
                                bararchive.BARHeader.Flags = ArchiveFlags.Bar_Flag_ZTOC;
                        }

                        bararchive.AllowWhitespaceInFilenames = true;

                        foreach (string path in enumerable)
                        {
                            bararchive.AddFile(Path.Combine(unzipdir, path));
                        }

                        // Get the name of the directory
                        string directoryName = new DirectoryInfo(unzipdir).Name;

                        // Create a text file to write the paths to
                        StreamWriter writer = new(unzipdir + @"/files.txt");

                        // Get all files in the directory and its immediate subdirectories
                        string[] files = Directory.GetFiles(unzipdir, "*.*", SearchOption.AllDirectories);

                        // Loop through the files and write their paths to the text file
                        foreach (string file in files)
                        {
                            string relativePath = string.Concat("file=\"", file.Replace(unzipdir, string.Empty).AsSpan(1), "\"");
                            writer.WriteLine(relativePath.Replace(@"\", "/"));
                        }

                        writer.Close();

                        bararchive.AddFile(unzipdir + @"/files.txt");

                        bararchive.CreateManifest();

                        bararchive.Save();

                        bararchive = null;

                        if (mode == "sdat")
                        {
                            if (version2 == "on")
                                RunUnBAR.RunEncrypt(converterPath, rebardir + $"/{filename}.SHARC", rebardir + $"/{filename.ToLower()}.sdat");
                            else
                                RunUnBAR.RunEncrypt(converterPath, rebardir + $"/{filename}.BAR", rebardir + $"/{filename.ToLower()}.sdat");

                            using (FileStream zipStream = new(rebardir + $"/{filename}_Rebar.zip", FileMode.Create))
                            {
                                using ZipArchive archive = new(zipStream, ZipArchiveMode.Create);
                                // Add the first file to the archive
                                ZipArchiveEntry entry1 = archive.CreateEntry($"{filename.ToLower()}.sdat");
                                using (Stream entryStream = entry1.Open())
                                {
                                    using (FileStream fileStream = new(rebardir + $"/{filename.ToLower()}.sdat", FileMode.Open))
                                    {
                                        fileStream.CopyTo(entryStream);
                                        fileStream.Flush();
                                    }
                                    entryStream.Flush();
                                }

                                if (version2 == "on")
                                {
                                    // Add the second file to the archive
                                    ZipArchiveEntry entry2 = archive.CreateEntry($"{filename}.sharc.map");
                                    using Stream entryStream = entry2.Open();
                                    using (FileStream fileStream = new(rebardir + $"/{filename}.sharc.map", FileMode.Open))
                                    {
                                        fileStream.CopyTo(entryStream);
                                        fileStream.Flush();
                                    }
                                    entryStream.Flush();
                                }
                                else
                                {
                                    // Add the second file to the archive
                                    ZipArchiveEntry entry2 = archive.CreateEntry($"{filename}.bar.map");
                                    using Stream entryStream = entry2.Open();
                                    using (FileStream fileStream = new(rebardir + $"/{filename}.bar.map", FileMode.Open))
                                    {
                                        fileStream.CopyTo(entryStream);
                                        fileStream.Flush();
                                    }
                                    entryStream.Flush();
                                }
                            }

                            TasksResult.Add((File.ReadAllBytes(rebardir + $"/{filename}_Rebar.zip"), $"{filename}_Rebar.zip"));
                        }
                        else
                        {
                            using (FileStream zipStream = new(rebardir + $"/{filename}_Rebar.zip", FileMode.Create))
                            {
                                using ZipArchive archive = new(zipStream, ZipArchiveMode.Create);
                                if (version2 == "on")
                                {
                                    // Add the first file to the archive
                                    ZipArchiveEntry entry1 = archive.CreateEntry($"{filename}.SHARC");
                                    using (Stream entryStream = entry1.Open())
                                    {
                                        using (FileStream fileStream = new(rebardir + $"/{filename}.SHARC", FileMode.Open))
                                        {
                                            fileStream.CopyTo(entryStream);
                                            fileStream.Flush();
                                        }
                                        entryStream.Flush();
                                    }

                                    // Add the second file to the archive
                                    ZipArchiveEntry entry2 = archive.CreateEntry($"{filename}.sharc.map");
                                    using (Stream entryStream = entry2.Open())
                                    {
                                        using (FileStream fileStream = new(rebardir + $"/{filename}.sharc.map", FileMode.Open))
                                        {
                                            fileStream.CopyTo(entryStream);
                                            fileStream.Flush();
                                        }
                                        entryStream.Flush();
                                    }
                                }
                                else
                                {
                                    // Add the first file to the archive
                                    ZipArchiveEntry entry1 = archive.CreateEntry($"{filename}.BAR");
                                    using (Stream entryStream = entry1.Open())
                                    {
                                        using (FileStream fileStream = new(rebardir + $"/{filename}.BAR", FileMode.Open))
                                        {
                                            fileStream.CopyTo(entryStream);
                                            fileStream.Flush();
                                        }
                                        entryStream.Flush();
                                    }

                                    // Add the second file to the archive
                                    ZipArchiveEntry entry2 = archive.CreateEntry($"{filename}.bar.map");
                                    using (Stream entryStream = entry2.Open())
                                    {
                                        using (FileStream fileStream = new(rebardir + $"/{filename}.bar.map", FileMode.Open))
                                        {
                                            fileStream.CopyTo(entryStream);
                                            fileStream.Flush();
                                        }
                                        entryStream.Flush();
                                    }
                                }
                            }

                            TasksResult.Add((File.ReadAllBytes(rebardir + $"/{filename}_Rebar.zip"), $"{filename}_Rebar.zip"));
                        }

                        if (Directory.Exists(tempdir))
                            Directory.Delete(tempdir, true);

                        i++;
                        filedata.Flush();
                    }
                    ms.Flush();
                }


                if (Directory.Exists(maindir))
                    Directory.Delete(maindir, true);
            }

            if (TasksResult.Count > 0)
            {
                // Create a memory stream to hold the zip file content
                using MemoryStream memoryStream = new();
                // Create a ZipArchive in memory
                using (ZipArchive archive = new(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var item in TasksResult)
                    {
                        if (item.HasValue)
                        {
                            // Add files or content to the zip archive
                            if (item.Value.Item1 != null)
                                AddFileToZip(archive, item.Value.Item2, new MemoryStream(item.Value.Item1));
                        }
                    }
                }

                memoryStream.Position = 0;

                output = (memoryStream.ToArray(), $"MakeBarSdat_Results.zip");
            }

            return output;
        }

        public static async Task<(byte[]?, string)?> UnBar(string converterPath, Stream? PostData, string? ContentType, string HelperStaticFolder)
        {
            (byte[]?, string)? output = null;
            List<(byte[]?, string)?> TasksResult = new();

            if (PostData != null && !string.IsNullOrEmpty(ContentType))
            {
                string maindir = Directory.GetCurrentDirectory() + $"/static/HomeToolsCache/UnBar_cache/{GenerateDynamicCacheGuid(VariousUtils.GetCurrentDateTime())}";
                Directory.CreateDirectory(maindir);
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);
                if (!string.IsNullOrEmpty(boundary))
                {
                    using MemoryStream ms = new();
                    PostData.CopyTo(ms);
                    ms.Position = 0;
                    int i = 0;
                    string filename = string.Empty;
                    string ogfilename = string.Empty;
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    string prefix = data.GetParameterValue("prefix");
                    string subfolder = string.Empty;
                    string bruteforce = string.Empty;
                    string afsengine = string.Empty;
                    try
                    {
                        subfolder = data.GetParameterValue("subfolder");
                    }
                    catch (Exception)
                    {
                        // Not Important
                    }
                    try
                    {
                        bruteforce = data.GetParameterValue("bruteforce");
                    }
                    catch (Exception)
                    {
                        // Not Important
                    }
                    try
                    {
                        afsengine = data.GetParameterValue("afsengine");
                    }
                    catch (Exception)
                    {
                        // Not Important
                    }
                    foreach (FilePart? multipartfile in data.Files)
                    {
                        using Stream filedata = multipartfile.Data;
                        filedata.Position = 0;

                        // Find the number of bytes in the stream
                        int contentLength = (int)filedata.Length;

                        // Create a byte array
                        byte[] buffer = new byte[contentLength];

                        // Read the contents of the memory stream into the byte array
                        filedata.Read(buffer, 0, contentLength);

                        filename = multipartfile.FileName;

                        string guid = GenerateDynamicCacheGuid(filename);

                        string tempdir = $"{maindir}/{guid}";

                        string unbardir = tempdir + $"/unbar";

                        string barfile = tempdir + $"/{filename}";

                        Directory.CreateDirectory(unbardir);

                        File.WriteAllBytes(barfile, buffer);

                        if (filename.ToLower().EndsWith(".bar") || filename.ToLower().EndsWith(".dat"))
                        {
                            await RunUnBAR.Run(converterPath, barfile, unbardir, false);
                            ogfilename = filename;
                            filename = filename[..^4].ToUpper();
                        }
                        else if (filename.ToLower().EndsWith(".sharc"))
                        {
                            await RunUnBAR.Run(converterPath, barfile, unbardir, false);
                            ogfilename = filename;
                            filename = filename[..^6].ToUpper();
                        }
                        else if (filename.ToLower().EndsWith(".sdat"))
                        {
                            await RunUnBAR.Run(converterPath, barfile, unbardir, true);
                            ogfilename = filename;
                            filename = filename[..^5].ToUpper();
                        }
                        else if (filename.ToLower().EndsWith(".zip"))
                        {
                            UncompressFile(barfile, unbardir);
                            ogfilename = filename;
                            filename = filename[..^4].ToUpper();
                        }
                        else
                        {
                            if (Directory.Exists(tempdir))
                                Directory.Delete(tempdir, true);

                            i++;
                            filedata.Flush();
                            continue;
                        }

                        LegacyMapper? map = new();

                        if (Directory.Exists(unbardir + $"/{filename}") && (ogfilename.ToLower().EndsWith(".bar") || ogfilename.ToLower().EndsWith(".sharc") || ogfilename.ToLower().EndsWith(".sdat")))
                        {
                            if (subfolder == "on")
                            {
                                foreach (var dircursor in Directory.GetDirectories(unbardir + $"/{filename}"))
                                {
                                    int fileCount = Directory.GetFiles(dircursor).Length;

                                    if (fileCount > 0)
                                    {
                                        if (afsengine == "on")
                                            await AFSClass.AFSMapStart(dircursor, prefix, bruteforce);
                                        else
                                            await map.MapperStart(dircursor, HelperStaticFolder, prefix, bruteforce);
                                    }
                                }
                            }
                            else
                            {
                                int fileCount = Directory.GetFiles(unbardir + $"/{filename}").Length;

                                if (fileCount > 0)
                                {
                                    if (afsengine == "on")
                                        await AFSClass.AFSMapStart(unbardir + $"/{filename}", prefix, bruteforce);
                                    else
                                        await map.MapperStart(unbardir + $"/{filename}", HelperStaticFolder, prefix, bruteforce);
                                }
                            }

                            ZipFile.CreateFromDirectory(unbardir + $"/{filename}", tempdir + $"/{filename}_Mapped.zip");

                            TasksResult.Add((File.ReadAllBytes(tempdir + $"/{filename}_Mapped.zip"), $"{filename}_Mapped.zip"));
                        }
                        else if (Directory.Exists(unbardir + $"/{filename}"))
                        {
                            if (subfolder == "on")
                            {
                                foreach (var dircursor in Directory.GetDirectories(unbardir + $"/{filename}"))
                                {
                                    int fileCount = Directory.GetFiles(dircursor).Length;

                                    if (fileCount > 0)
                                    {
                                        if (afsengine == "on")
                                            await AFSClass.AFSMapStart(dircursor, prefix, bruteforce);
                                        else
                                            await map.MapperStart(dircursor, HelperStaticFolder, prefix, bruteforce);
                                    }
                                }
                            }
                            else
                            {
                                int fileCount = Directory.GetFiles(unbardir + $"/{filename}").Length;

                                if (fileCount > 0)
                                {
                                    if (afsengine == "on")
                                        await AFSClass.AFSMapStart(unbardir + $"/{filename}", prefix, bruteforce);
                                    else
                                        await map.MapperStart(unbardir + $"/{filename}", HelperStaticFolder, prefix, bruteforce);
                                }
                            }

                            ZipFile.CreateFromDirectory(unbardir + $"/{filename}", tempdir + $"/{filename}_Mapped.zip");

                            TasksResult.Add((File.ReadAllBytes(tempdir + $"/{filename}_Mapped.zip"), $"{filename}_Mapped.zip"));
                        }
                        else
                        {
                            if (subfolder == "on")
                            {
                                foreach (var dircursor in Directory.GetDirectories(unbardir))
                                {
                                    int fileCount = Directory.GetFiles(dircursor).Length;

                                    if (fileCount > 0)
                                    {
                                        if (afsengine == "on")
                                            await AFSClass.AFSMapStart(dircursor, prefix, bruteforce);
                                        else
                                            await map.MapperStart(dircursor, HelperStaticFolder, prefix, bruteforce);
                                    }
                                }
                            }
                            else
                            {
                                int fileCount = Directory.GetFiles(unbardir).Length;

                                if (fileCount > 0)
                                {
                                    if (afsengine == "on")
                                        await AFSClass.AFSMapStart(unbardir, prefix, bruteforce);
                                    else
                                        await map.MapperStart(unbardir, HelperStaticFolder, prefix, bruteforce);
                                }
                            }

                            ZipFile.CreateFromDirectory(unbardir, tempdir + $"/{filename}_Mapped.zip");

                            TasksResult.Add((File.ReadAllBytes(tempdir + $"/{filename}_Mapped.zip"), $"{filename}_Mapped.zip"));
                        }

                        map = null;

                        if (Directory.Exists(tempdir))
                            Directory.Delete(tempdir, true);

                        i++;
                        filedata.Flush();
                    }
                    ms.Flush();
                }

                if (Directory.Exists(maindir))
                    Directory.Delete(maindir, true);
            }

            if (TasksResult.Count > 0)
            {
                // Create a memory stream to hold the zip file content
                using MemoryStream memoryStream = new();
                // Create a ZipArchive in memory
                using (ZipArchive archive = new(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var item in TasksResult)
                    {
                        if (item.HasValue)
                        {
                            // Add files or content to the zip archive
                            if (item.Value.Item1 != null)
                                AddFileToZip(archive, item.Value.Item2, new MemoryStream(item.Value.Item1));
                        }
                    }
                }

                memoryStream.Position = 0;

                output = (memoryStream.ToArray(), $"UnBar_Results.zip");
            }

            return output;
        }

        public static (byte[]?, string)? CDS(Stream? PostData, string? ContentType)
        {
            (byte[]?, string)? output = null;
            List<(byte[]?, string)?> TasksResult = new();

            if (PostData != null && !string.IsNullOrEmpty(ContentType))
            {
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);
                if (!string.IsNullOrEmpty(boundary))
                {
                    using MemoryStream ms = new();
                    PostData.CopyTo(ms);
                    ms.Position = 0;
                    int i = 0;
                    string filename = string.Empty;
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    string? decrypt = string.Empty;
                    string? sha1 = data.GetParameterValue("sha1");
                    try
                    {
                        decrypt = data.GetParameterValue("decrypt");
                    }
                    catch (Exception)
                    {
                        // Not Important
                    }
                    foreach (FilePart? multipartfile in data.Files)
                    {
                        using Stream filedata = multipartfile.Data;
                        filedata.Position = 0;

                        // Find the number of bytes in the stream
                        int contentLength = (int)filedata.Length;

                        // Create a byte array
                        byte[] buffer = new byte[contentLength];

                        // Read the contents of the memory stream into the byte array
                        filedata.Read(buffer, 0, contentLength);

                        filename = multipartfile.FileName;

                        if (decrypt == "on" && sha1.Length >= 16)
                        {
                            byte[]? ProcessedFileBytes = CDSProcess.CDSEncrypt_Decrypt(buffer, sha1[..16]);

                            if (ProcessedFileBytes != null)
                            {
                                if (ProcessedFileBytes.Length >= 8 && (ProcessedFileBytes[0] == 0x3c && ProcessedFileBytes[1] == 0x78 && ProcessedFileBytes[2] == 0x6d && ProcessedFileBytes[3] == 0x6c
                                    || ProcessedFileBytes[0] == 0x3c && ProcessedFileBytes[1] == 0x58 && ProcessedFileBytes[2] == 0x4d && ProcessedFileBytes[3] == 0x4c
                                    || ProcessedFileBytes[0] == 0xEF && ProcessedFileBytes[1] == 0xBB && ProcessedFileBytes[2] == 0xBF && ProcessedFileBytes[3] == 0x3C && ProcessedFileBytes[4] == 0x3F && ProcessedFileBytes[5] == 0x78 && ProcessedFileBytes[6] == 0x6D && ProcessedFileBytes[7] == 0x6C
                                    || ProcessedFileBytes[0] == 0x3C && ProcessedFileBytes[1] == 0x3F && ProcessedFileBytes[2] == 0x78 && ProcessedFileBytes[3] == 0x6D && ProcessedFileBytes[4] == 0x6C && ProcessedFileBytes[5] == 0x20 && ProcessedFileBytes[6] == 0x76 && ProcessedFileBytes[7] == 0x65
                                    || ProcessedFileBytes[0] == 0x3c && ProcessedFileBytes[1] == 0x53 && ProcessedFileBytes[2] == 0x43 && ProcessedFileBytes[3] == 0x45))
                                {
                                    if (filename.ToLower().Contains(".sdc"))
                                        TasksResult.Add((ProcessedFileBytes, $"{filename}_Decrypted.sdc"));
                                    else if (filename.ToLower().Contains(".odc"))
                                        TasksResult.Add((ProcessedFileBytes, $"{filename}_Decrypted.odc"));
                                    else
                                        TasksResult.Add((ProcessedFileBytes, $"{filename}_Decrypted.xml"));
                                }
                                else if (ProcessedFileBytes.Length > 4 && ProcessedFileBytes[0] == 0x73 && ProcessedFileBytes[1] == 0x65 && ProcessedFileBytes[2] == 0x67 && ProcessedFileBytes[3] == 0x73)
                                    TasksResult.Add((ProcessedFileBytes, $"{filename}_Decrypted.hcdb"));
                                else if (ProcessedFileBytes.Length > 4 && ((ProcessedFileBytes[0] == 0xAD && ProcessedFileBytes[1] == 0xEF && ProcessedFileBytes[2] == 0x17 && ProcessedFileBytes[3] == 0xE1)
                                    || (ProcessedFileBytes[0] == 0xE1 && ProcessedFileBytes[1] == 0x17 && ProcessedFileBytes[2] == 0xEF && ProcessedFileBytes[3] == 0xAD)))
                                    TasksResult.Add((ProcessedFileBytes, $"{filename}_Decrypted.bar"));
                                else // If all scan failed, fallback.
                                    TasksResult.Add((ProcessedFileBytes, $"{filename}_Decrypted.bin"));
                            }
                        }
                        else
                        {
                            using SHA1 sha1hash = SHA1.Create();
                            byte[]? ProcessedFileBytes = CDSProcess.CDSEncrypt_Decrypt(buffer, BitConverter.ToString(sha1hash.ComputeHash(buffer)).Replace("-", "").ToUpper()[..16]);

                            if (ProcessedFileBytes != null)
                            {
                                if (buffer.Length >= 8 && (buffer[0] == 0x3c && buffer[1] == 0x78 && buffer[2] == 0x6d && buffer[3] == 0x6c
                                    || buffer[0] == 0x3c && buffer[1] == 0x58 && buffer[2] == 0x4d && buffer[3] == 0x4c
                                    || buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF && buffer[3] == 0x3C && buffer[4] == 0x3F && buffer[5] == 0x78 && buffer[6] == 0x6D && buffer[7] == 0x6C
                                    || buffer[0] == 0x3C && buffer[1] == 0x3F && buffer[2] == 0x78 && buffer[3] == 0x6D && buffer[4] == 0x6C && buffer[5] == 0x20 && buffer[6] == 0x76 && buffer[7] == 0x65
                                    || buffer[0] == 0x3c && buffer[1] == 0x53 && buffer[2] == 0x43 && buffer[3] == 0x45))
                                {
                                    if (filename.ToLower().Contains(".sdc"))
                                        TasksResult.Add((ProcessedFileBytes, $"{filename}_Encrypted.sdc"));
                                    else if (filename.ToLower().Contains(".odc"))
                                        TasksResult.Add((ProcessedFileBytes, $"{filename}_Encrypted.odc"));
                                    else
                                        TasksResult.Add((ProcessedFileBytes, $"{filename}_Encrypted.xml"));
                                }
                                else if (buffer.Length > 4 && buffer[0] == 0x73 && buffer[1] == 0x65 && buffer[2] == 0x67 && buffer[3] == 0x73)
                                    TasksResult.Add((ProcessedFileBytes, $"{filename}_Encrypted.hcdb"));
                                else if (buffer.Length > 4 && ((buffer[0] == 0xAD && buffer[1] == 0xEF && buffer[2] == 0x17 && buffer[3] == 0xE1)
                                    || (buffer[0] == 0xE1 && buffer[1] == 0x17 && buffer[2] == 0xEF && buffer[3] == 0xAD)))
                                    TasksResult.Add((ProcessedFileBytes, $"{filename}_Encrypted.bar"));
                                else // If all scan failed, fallback.
                                    TasksResult.Add((ProcessedFileBytes, $"{filename}_Encrypted.bin"));
                            }

                            sha1hash.Clear();
                        }

                        i++;
                        filedata.Flush();
                    }
                    ms.Flush();
                }
            }

            if (TasksResult.Count > 0)
            {
                // Create a memory stream to hold the zip file content
                using MemoryStream memoryStream = new();
                // Create a ZipArchive in memory
                using (ZipArchive archive = new(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var item in TasksResult)
                    {
                        if (item.HasValue)
                        {
                            // Add files or content to the zip archive
                            if (item.Value.Item1 != null)
                                AddFileToZip(archive, item.Value.Item2, new MemoryStream(item.Value.Item1));
                        }
                    }
                }

                memoryStream.Position = 0;

                output = (memoryStream.ToArray(), $"CDS_Results.zip");
            }

            return output;
        }

        public static (byte[]?, string)? CDSBruteforce(Stream? PostData, string? ContentType)
        {
            (byte[]?, string)? output = null;
            List<(byte[]?, string)?> TasksResult = new();

            if (PostData != null && !string.IsNullOrEmpty(ContentType))
            {
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);
                if (!string.IsNullOrEmpty(boundary))
                {
                    using MemoryStream ms = new();
                    PostData.CopyTo(ms);
                    ms.Position = 0;
                    int i = 0;
                    string filename = string.Empty;
                    foreach (FilePart? multipartfile in MultipartFormDataParser.Parse(ms, boundary).Files)
                    {
                        using Stream filedata = multipartfile.Data;
                        filedata.Position = 0;

                        // Find the number of bytes in the stream
                        int contentLength = (int)filedata.Length;

                        // Create a byte array
                        byte[] buffer = new byte[contentLength];

                        // Read the contents of the memory stream into the byte array
                        filedata.Read(buffer, 0, contentLength);

                        filename = multipartfile.FileName;

                        BruteforceProcess? proc = new(buffer);

                        if (filename.ToLower().Contains(".hcdb"))
                            TasksResult.Add((proc.StartBruteForce(1), $"{filename}_Bruteforced.hcdb"));
                        else if (filename.ToLower().Contains(".bar"))
                            TasksResult.Add((proc.StartBruteForce(2), $"{filename}_Bruteforced.bar"));
                        else
                            TasksResult.Add((proc.StartBruteForce(), $"{filename}_Bruteforced.xml"));

                        proc = null;

                        i++;
                        filedata.Flush();
                    }
                    ms.Flush();
                }
            }

            if (TasksResult.Count > 0)
            {
                // Create a memory stream to hold the zip file content
                using MemoryStream memoryStream = new();
                // Create a ZipArchive in memory
                using (ZipArchive archive = new(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var item in TasksResult)
                    {
                        if (item.HasValue)
                        {
                            // Add files or content to the zip archive
                            if (item.Value.Item1 != null)
                                AddFileToZip(archive, item.Value.Item2, new MemoryStream(item.Value.Item1));
                        }
                    }
                }

                memoryStream.Position = 0;

                output = (memoryStream.ToArray(), $"CDSBruteforce_Results.zip");
            }

            return output;
        }

        public static (byte[]?, string)? HCDBUnpack(Stream? PostData, string? ContentType)
        {
            (byte[]?, string)? output = null;
            List<(byte[]?, string)?> TasksResult = new();

            if (PostData != null && !string.IsNullOrEmpty(ContentType))
            {
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);
                if (!string.IsNullOrEmpty(boundary))
                {
                    using MemoryStream ms = new();
                    PostData.CopyTo(ms);
                    ms.Position = 0;
                    int i = 0;
                    string filename = string.Empty;
                    foreach (FilePart? multipartfile in MultipartFormDataParser.Parse(ms, boundary).Files)
                    {
                        using Stream filedata = multipartfile.Data;
                        filedata.Position = 0;

                        // Find the number of bytes in the stream
                        int contentLength = (int)filedata.Length;

                        // Create a byte array
                        byte[] buffer = new byte[contentLength];

                        // Read the contents of the memory stream into the byte array
                        filedata.Read(buffer, 0, contentLength);

                        filename = multipartfile.FileName;

                        byte[]? DecompressedData = new EdgeLZMAUtils().Decompress(buffer, true);

                        if (DecompressedData != null && DecompressedData[0] != 0x73 && DecompressedData[1] != 0x65 && DecompressedData[2] != 0x67 && DecompressedData[3] != 0x73)
                            TasksResult.Add((DecompressedData, $"{filename}_Unpacked.sql"));

                        i++;
                        filedata.Flush();
                    }
                    ms.Flush();
                }
            }

            if (TasksResult.Count > 0)
            {
                // Create a memory stream to hold the zip file content
                using MemoryStream memoryStream = new();
                // Create a ZipArchive in memory
                using (ZipArchive archive = new(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var item in TasksResult)
                    {
                        if (item.HasValue)
                        {
                            // Add files or content to the zip archive
                            if (item.Value.Item1 != null)
                                AddFileToZip(archive, item.Value.Item2, new MemoryStream(item.Value.Item1));
                        }
                    }
                }

                memoryStream.Position = 0;

                output = (memoryStream.ToArray(), $"HCDBUnpack_Results.zip");
            }

            return output;
        }

        public static (byte[]?, string)? TicketList(Stream? PostData, string? ContentType)
        {
            (byte[]?, string)? output = null;
            List<(byte[]?, string)?> TasksResult = new();

            if (PostData != null && !string.IsNullOrEmpty(ContentType))
            {
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);
                if (!string.IsNullOrEmpty(boundary))
                {
                    using MemoryStream ms = new();
                    PostData.CopyTo(ms);
                    ms.Position = 0;
                    int i = 0;
                    string filename = string.Empty;
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    string version1 = string.Empty;
                    try
                    {
                        version1 = data.GetParameterValue("version1");
                    }
                    catch (Exception)
                    {
                        // Not Important
                    }
                    foreach (FilePart? multipartfile in data.Files)
                    {
                        using Stream filedata = multipartfile.Data;
                        filedata.Position = 0;

                        // Find the number of bytes in the stream
                        int contentLength = (int)filedata.Length;

                        // Create a byte array
                        byte[] buffer = new byte[contentLength];

                        // Read the contents of the memory stream into the byte array
                        filedata.Read(buffer, 0, contentLength);

                        filename = multipartfile.FileName;

                        string guid = GenerateDynamicCacheGuid(filename);

                        if (buffer.Length > 8 && buffer[0] == 0xBE && buffer[1] == 0xE5 && buffer[2] == 0xBE && buffer[3] == 0xE5
                             && buffer[4] == 0x00 && buffer[5] == 0x00 && buffer[6] == 0x00 && buffer[7] == 0x01 && version1 == "on")
                        {
                            byte[]? ProcessedFileBytes = new byte[buffer.Length - 8];
                            Buffer.BlockCopy(buffer, 8, ProcessedFileBytes, 0, ProcessedFileBytes.Length);
                            ProcessedFileBytes = new BlowfishCTREncryptDecrypt().TicketListV1Process(ProcessedFileBytes);
                            if (ProcessedFileBytes != null)
                                TasksResult.Add((ProcessedFileBytes, $"{filename}_Decrypted.lst"));
                        }
                        else if (version1 == "on")
                            TasksResult.Add((VariousUtils.CombineByteArray(new byte[] { 0xBE, 0xE5, 0xBE, 0xE5, 0x00, 0x00, 0x00, 0x01 }, new BlowfishCTREncryptDecrypt().TicketListV1Process(buffer))
                                    , $"{filename}_Encrypted.lst"));
                        else if (buffer.Length > 8 && buffer[0] == 0xBE && buffer[1] == 0xE5 && buffer[2] == 0xBE && buffer[3] == 0xE5
                            && buffer[4] == 0x00 && buffer[5] == 0x00 && buffer[6] == 0x00 && buffer[7] == 0x00)
                        {
                            byte[]? ProcessedFileBytes = new byte[buffer.Length - 8];
                            Buffer.BlockCopy(buffer, 8, ProcessedFileBytes, 0, ProcessedFileBytes.Length);
                            ProcessedFileBytes = new BlowfishCTREncryptDecrypt().TicketListV0Process(ProcessedFileBytes);
                            if (ProcessedFileBytes != null)
                                TasksResult.Add((ProcessedFileBytes, $"{filename}_Decrypted.lst"));
                        }
                        else
                            TasksResult.Add((VariousUtils.CombineByteArray(new byte[] { 0xBE, 0xE5, 0xBE, 0xE5, 0x00, 0x00, 0x00, 0x00 }, new BlowfishCTREncryptDecrypt().TicketListV0Process(buffer))
                                    , $"{filename}_Encrypted.lst"));

                        i++;
                        filedata.Flush();
                    }
                    ms.Flush();
                }
            }

            if (TasksResult.Count > 0)
            {
                // Create a memory stream to hold the zip file content
                using MemoryStream memoryStream = new();
                // Create a ZipArchive in memory
                using (ZipArchive archive = new(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var item in TasksResult)
                    {
                        if (item.HasValue)
                        {
                            // Add files or content to the zip archive
                            if (item.Value.Item1 != null)
                                AddFileToZip(archive, item.Value.Item2, new MemoryStream(item.Value.Item1));
                        }
                    }
                }

                memoryStream.Position = 0;

                output = (memoryStream.ToArray(), $"TicketList_Results.zip");
            }

            return output;
        }

        public static (byte[]?, string)? INF(Stream? PostData, string? ContentType)
        {
            (byte[]?, string)? output = null;
            List<(byte[]?, string)?> TasksResult = new();

            if (ToolsImpl.INFIVA == null)
                BlowfishCTREncryptDecrypt.InitiateMetadataCryptoContext();

            if (PostData != null && !string.IsNullOrEmpty(ContentType))
            {
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);
                if (!string.IsNullOrEmpty(boundary))
                {
                    using MemoryStream ms = new();
                    PostData.CopyTo(ms);
                    ms.Position = 0;
                    int i = 0;
                    string filename = string.Empty;
                    foreach (FilePart? multipartfile in MultipartFormDataParser.Parse(ms, boundary).Files)
                    {
                        using Stream filedata = multipartfile.Data;
                        filedata.Position = 0;

                        // Find the number of bytes in the stream
                        int contentLength = (int)filedata.Length;

                        // Create a byte array
                        byte[] buffer = new byte[contentLength];

                        // Read the contents of the memory stream into the byte array
                        filedata.Read(buffer, 0, contentLength);

                        filename = multipartfile.FileName;

                        if (buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0x00 && buffer[3] == 0x01 && ToolsImpl.INFIVA != null)
                        {
                            ToolsImpl? toolsimpl = new();

                            buffer = toolsimpl.RemovePaddingPrefix(buffer);

                            byte[]? decryptedfilebytes = toolsimpl.Crypt_Decrypt(buffer, ToolsImpl.INFIVA, 8);

                            toolsimpl = null;

                            if (decryptedfilebytes != null)
                                TasksResult.Add((decryptedfilebytes, $"{filename}_Decrypted.bin"));
                        }
                        else if (buffer[0] == 0xBE && buffer[1] == 0xE5 && buffer[2] == 0xBE && buffer[3] == 0xE5 && ToolsImpl.INFIVA != null)
                        {
                            ToolsImpl? toolsimpl = new();

                            byte[]? encryptedfilebytes = toolsimpl.Crypt_Decrypt(buffer, ToolsImpl.INFIVA, 8);

                            if (encryptedfilebytes != null)
                                TasksResult.Add((toolsimpl.ApplyLittleEndianPaddingPrefix(encryptedfilebytes), $"{filename}_Encrypted.bin"));

                            toolsimpl = null;
                        }

                        i++;
                        filedata.Flush();
                    }
                    ms.Flush();
                }
            }

            if (TasksResult.Count > 0)
            {
                // Create a memory stream to hold the zip file content
                using MemoryStream memoryStream = new();
                // Create a ZipArchive in memory
                using (ZipArchive archive = new(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var item in TasksResult)
                    {
                        if (item.HasValue)
                        {
                            // Add files or content to the zip archive
                            if (item.Value.Item1 != null)
                                AddFileToZip(archive, item.Value.Item2, new MemoryStream(item.Value.Item1));
                        }
                    }
                }

                memoryStream.Position = 0;

                output = (memoryStream.ToArray(), $"INF_Results.zip");
            }

            return output;
        }

        public static string? ChannelID(Stream? PostData, string? ContentType)
        {
            string? res = null;

            if (PostData != null && !string.IsNullOrEmpty(ContentType))
            {
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);
                if (!string.IsNullOrEmpty(boundary))
                {
                    using MemoryStream ms = new();
                    PostData.CopyTo(ms);
                    ms.Position = 0;
                    int sceneid = 0;
                    string newerhome = string.Empty;
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    try
                    {
                        sceneid = int.Parse(data.GetParameterValue("sceneid"));
                    }
                    catch (Exception)
                    {
                        // Not Important
                    }
                    try
                    {
                        newerhome = data.GetParameterValue("newerhome");
                    }
                    catch (Exception)
                    {
                        // Not Important
                    }
                    if (newerhome == "on")
                    {
                        SceneKey? sceneKey = SIDKeyGenerator.Instance.GenerateNewerType(Convert.ToUInt16(sceneid));
                        res = sceneKey.ToString();
                        sceneKey = null;
                    }
                    else
                    {
                        SceneKey? sceneKey = SIDKeyGenerator.Instance.Generate(Convert.ToUInt16(sceneid));
                        res = sceneKey.ToString();
                        sceneKey = null;
                    }
                    ms.Flush();
                }
            }

            return res;
        }

        public static string? SceneID(Stream? PostData, string? ContentType)
        {
            string? res = null;

            if (PostData != null && !string.IsNullOrEmpty(ContentType))
            {
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);
                if (!string.IsNullOrEmpty(boundary))
                {
                    using MemoryStream ms = new();
                    PostData.CopyTo(ms);
                    ms.Position = 0;
                    string newerhome = string.Empty;
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    string channelid = data.GetParameterValue("channelid");
                    try
                    {
                        newerhome = data.GetParameterValue("newerhome");
                    }
                    catch (Exception)
                    {

                    }
                    if (newerhome == "on")
                    {
                        SceneKey? sceneKey = new(channelid);
                        try
                        {
                            SIDKeyGenerator.Instance.VerifyNewerKey(sceneKey);
                            ushort num = SIDKeyGenerator.Instance.ExtractSceneIDNewerType(sceneKey);
                            res = num.ToString();
                        }
                        catch (SceneKeyException)
                        {
                            res = "Invalid ChannelID";
                        }
                        catch (Exception)
                        {
                            // Not Important
                        }
                        sceneKey = null;
                    }
                    else
                    {
                        SceneKey? sceneKey = new(channelid);
                        try
                        {
                            SIDKeyGenerator.Instance.Verify(sceneKey);
                            ushort num = SIDKeyGenerator.Instance.ExtractSceneID(sceneKey);
                            res = num.ToString();
                        }
                        catch (SceneKeyException)
                        {
                            res = "Invalid ChannelID";
                        }
                        catch (Exception)
                        {
                            // Not Important
                        }
                        sceneKey = null;
                    }
                    ms.Flush();
                }
            }

            return res;
        }

        public static string GenerateDynamicCacheGuid(string input)
        {
            return BitConverter.ToString(MD5.HashData(Encoding.UTF8.GetBytes(VariousUtils.GetCurrentDateTime() + input))).Replace("-", string.Empty);
        }

        private static void AddFileToZip(ZipArchive archive, string entryName, Stream contentStream)
        {
            contentStream.Position = 0;

            // Create a new entry in the zip archive
            ZipArchiveEntry entry = archive.CreateEntry(entryName);

            // Write content to the entry
            using Stream entryStream = entry.Open();
            contentStream.CopyTo(entryStream);
        }

        private static void UncompressFile(string compressedFilePath, string extractionFolderPath)
        {
            try
            {
                ZipFile.ExtractToDirectory(compressedFilePath, extractionFolderPath);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[File Uncompress] - An error occurred: {ex}");
            }
        }
    }
}
