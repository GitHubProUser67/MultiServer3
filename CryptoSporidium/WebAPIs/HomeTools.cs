using CryptoSporidium.BARTools.BAR;
using CryptoSporidium.BARTools.UnBAR;
using CryptoSporidium.WebAPIs.CDS;
using CryptoSporidium.WebAPIs.ChannelID;
using CustomLogger;
using HttpMultipartParser;
using System.IO.Compression;

namespace CryptoSporidium.WebAPIs
{
    public class HomeTools
    {
        public static (byte[]?, string)? MakeBarSdat(Stream? PostData, string? ContentType)
        {
            (byte[]?, string)? output = null;

            if (PostData != null && !string.IsNullOrEmpty(ContentType))
            {
                string maindir = Directory.GetCurrentDirectory() + $"/static/HomeToolsCache/MakeBarSdat_cache/{MiscUtils.GenerateDynamicCacheGuid(MiscUtils.GetCurrentDateTime())}";
                Directory.CreateDirectory(maindir);
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);
                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new())
                    {
                        PostData.CopyTo(ms);
                        ms.Position = 0;
                        int i = 0;
                        string filename = string.Empty;
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        string mode = data.GetParameterValue("mode");
                        string TimeStamp = data.GetParameterValue("TimeStamp");
                        string leanzlib = string.Empty;
                        string encrypt = string.Empty;
                        try
                        {
                            leanzlib = data.GetParameterValue("leanzlib");
                        }
                        catch (Exception)
                        {

                        }
                        try
                        {
                            encrypt = data.GetParameterValue("encrypt");
                        }
                        catch (Exception)
                        {

                        }
                        foreach (var multipartfile in data.Files)
                        {
                            using (Stream filedata = multipartfile.Data)
                            {
                                filedata.Position = 0;

                                // Find the number of bytes in the stream
                                int contentLength = (int)filedata.Length;

                                // Create a byte array
                                byte[] buffer = new byte[contentLength];

                                // Read the contents of the memory stream into the byte array
                                filedata.Read(buffer, 0, contentLength);

                                filename = multipartfile.FileName;

                                string guid = MiscUtils.GenerateDynamicCacheGuid(filename);

                                string tempdir = $"{maindir}/{guid}";

                                string unzipdir = tempdir + $"/unzip";

                                string rebardir = tempdir + $"/archive";

                                string zipfile = tempdir + $"/{filename}";

                                if (Directory.Exists(tempdir))
                                    Directory.Delete(tempdir, true);

                                Directory.CreateDirectory(rebardir);

                                File.WriteAllBytes(zipfile, buffer);

                                UncompressFile(zipfile, unzipdir);

                                filename = filename.Substring(0, filename.Length - 4).ToUpper();

                                IEnumerable<string> enumerable = Directory.EnumerateFiles(unzipdir, "*.*", SearchOption.AllDirectories);
                                BARArchive? bararchive = null;
                                if (encrypt == "on")
                                    bararchive = new BARArchive(string.Format("{0}/{1}.BAR", rebardir, filename), unzipdir, Convert.ToInt32(TimeStamp, 16), true);
                                else
                                    bararchive = new BARArchive(string.Format("{0}/{1}.BAR", rebardir, filename), unzipdir, Convert.ToInt32(TimeStamp, 16));
                                if (leanzlib == "on")
                                {
                                    bararchive.BARHeader.Flags = ArchiveFlags.Bar_Flag_LeanZLib;
                                    bararchive.DefaultCompression = CompressionMethod.ZLib;
                                }
                                else
                                {
                                    bararchive.BARHeader.Flags = ArchiveFlags.Bar_Flag_ZTOC;
                                    bararchive.DefaultCompression = CompressionMethod.EdgeZLib;
                                }
                                bararchive.AllowWhitespaceInFilenames = true;
                                bararchive.BeginUpdate();
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
                                    string relativePath = "file=\"" + file.Replace(unzipdir, string.Empty).Substring(1) + "\"";
                                    writer.WriteLine(relativePath.Replace(@"\", "/"));
                                }

                                writer.Close();

                                bararchive.AddFile(unzipdir + @"/files.txt");

                                bararchive.CreateManifest();

                                bararchive.Save();

                                bararchive = null;

                                FileHelper.FileHelper filehelper = new();

                                if (mode == "bar")
                                {
                                    using (FileStream zipStream = new(rebardir + $"/{filename}_Rebar.zip", FileMode.Create))
                                    {
                                        using (ZipArchive archive = new(zipStream, ZipArchiveMode.Create))
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

                                    output = (File.ReadAllBytes(rebardir + $"/{filename}_Rebar.zip"), $"{filename}_Rebar.zip");
                                }
                                else if (mode == "sdatnpd" && File.Exists(Directory.GetCurrentDirectory() + "/static/model.sdat"))
                                {
                                    new RunUnBAR().RunEncrypt(rebardir + $"/{filename}.BAR", rebardir + $"/{filename.ToLower()}.sdat", Directory.GetCurrentDirectory() + "/static/model.sdat");

                                    using (FileStream zipStream = new(rebardir + $"/{filename}_Rebar.zip", FileMode.Create))
                                    {
                                        using (ZipArchive archive = new(zipStream, ZipArchiveMode.Create))
                                        {
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

                                    output = (File.ReadAllBytes(rebardir + $"/{filename}_Rebar.zip"), $"{filename}_Rebar.zip");
                                }
                                else
                                {
                                    new RunUnBAR().RunEncrypt(rebardir + $"/{filename}.BAR", rebardir + $"/{filename.ToLower()}.sdat", null);

                                    using (FileStream zipStream = new(rebardir + $"/{filename}_Rebar.zip", FileMode.Create))
                                    {
                                        using (ZipArchive archive = new(zipStream, ZipArchiveMode.Create))
                                        {
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

                                    output = (File.ReadAllBytes(rebardir + $"/{filename}_Rebar.zip"), $"{filename}_Rebar.zip");
                                }

                                if (Directory.Exists(tempdir))
                                    Directory.Delete(tempdir, true);

                                i++;
                                filedata.Flush();
                            }
                        }
                        ms.Flush();
                    }
                }


                if (Directory.Exists(maindir))
                    Directory.Delete(maindir, true);
            }

            return output;
        }

        public static async Task<(byte[]?, string)?> UnBar(Stream? PostData, string? ContentType, string HelperStaticFolder)
        {
            (byte[]?, string)? output = null;

            if (PostData != null && !string.IsNullOrEmpty(ContentType))
            {
                string maindir = Directory.GetCurrentDirectory() + $"/static/HomeToolsCache/UnBar_cache/{MiscUtils.GenerateDynamicCacheGuid(MiscUtils.GetCurrentDateTime())}";
                Directory.CreateDirectory(maindir);
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);
                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new())
                    {
                        PostData.CopyTo(ms);
                        ms.Position = 0;
                        int i = 0;
                        string filename = string.Empty;
                        string ogfilename = string.Empty;
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        string options = data.GetParameterValue("options");
                        string prefix = data.GetParameterValue("prefix");
                        string subfolder = string.Empty;
                        string bruteforce = string.Empty;
                        try
                        {
                            subfolder = data.GetParameterValue("subfolder");
                        }
                        catch (Exception)
                        {

                        }
                        try
                        {
                            bruteforce = data.GetParameterValue("bruteforce");
                        }
                        catch (Exception)
                        {

                        }
                        if (options == "cdn1")
                            options = ToolsImpl.base64CDNKey1;
                        else if (options == "cdn2")
                            options = ToolsImpl.base64CDNKey2;
                        else
                            options = ToolsImpl.base64DefaultSharcKey;

                        foreach (var multipartfile in data.Files)
                        {
                            using (Stream filedata = multipartfile.Data)
                            {
                                filedata.Position = 0;

                                // Find the number of bytes in the stream
                                int contentLength = (int)filedata.Length;

                                // Create a byte array
                                byte[] buffer = new byte[contentLength];

                                // Read the contents of the memory stream into the byte array
                                filedata.Read(buffer, 0, contentLength);

                                filename = multipartfile.FileName;

                                string guid = MiscUtils.GenerateDynamicCacheGuid(filename);

                                string tempdir = $"{maindir}/{guid}";

                                string unbardir = tempdir + $"/unbar";

                                string barfile = tempdir + $"/{filename}";

                                Directory.CreateDirectory(unbardir);

                                File.WriteAllBytes(barfile, buffer);

                                RunUnBAR? unbar = new();

                                if (filename.ToLower().EndsWith(".bar") || filename.ToLower().EndsWith(".dat"))
                                {
                                    await unbar.Run(barfile, unbardir, false, options);
                                    ogfilename = filename;
                                    filename = filename.Substring(0, filename.Length - 4).ToUpper();
                                }
                                else if (filename.ToLower().EndsWith(".sharc"))
                                {
                                    await unbar.Run(barfile, unbardir, false, options);
                                    ogfilename = filename;
                                    filename = filename.Substring(0, filename.Length - 6).ToUpper();
                                }
                                else if (filename.ToLower().EndsWith(".sdat"))
                                {
                                    await unbar.Run(barfile, unbardir, true, options);
                                    ogfilename = filename;
                                    filename = filename.Substring(0, filename.Length - 5).ToUpper();
                                }
                                else if (filename.ToLower().EndsWith(".zip"))
                                {
                                    UncompressFile(barfile, unbardir);
                                    ogfilename = filename;
                                    filename = filename.Substring(0, filename.Length - 4).ToUpper();
                                }

                                unbar = null;

                                Mapper? map = new();

                                if (Directory.Exists(unbardir + $"/{filename}") && (ogfilename.ToLower().EndsWith(".bar") || ogfilename.ToLower().EndsWith(".sharc") || ogfilename.ToLower().EndsWith(".sdat")))
                                {
                                    if (subfolder == "on")
                                    {
                                        foreach (var dircursor in Directory.GetDirectories(unbardir + $"/{filename}"))
                                        {
                                            int fileCount = Directory.GetFiles(dircursor).Length;

                                            if (fileCount > 0)
                                                await map.MapperStart(dircursor, HelperStaticFolder, prefix, bruteforce);
                                        }
                                    }
                                    else
                                    {
                                        int fileCount = Directory.GetFiles(unbardir + $"/{filename}").Length;

                                        if (fileCount > 0)
                                            await map.MapperStart(unbardir + $"/{filename}", HelperStaticFolder, prefix, bruteforce);
                                    }


                                    ZipFile.CreateFromDirectory(unbardir + $"/{filename}", tempdir + $"/{filename}_Mapped.zip");

                                    output = (File.ReadAllBytes(tempdir + $"/{filename}_Mapped.zip"), $"{filename}_Mapped.zip");
                                }
                                else if (Directory.Exists(unbardir + $"/{filename}"))
                                {
                                    if (subfolder == "on")
                                    {
                                        foreach (var dircursor in Directory.GetDirectories(unbardir + $"/{filename}"))
                                        {
                                            int fileCount = Directory.GetFiles(dircursor).Length;

                                            if (fileCount > 0)
                                                await map.MapperStart(dircursor, HelperStaticFolder, prefix, bruteforce);
                                        }
                                    }
                                    else
                                    {
                                        int fileCount = Directory.GetFiles(unbardir + $"/{filename}").Length;

                                        if (fileCount > 0)
                                            await map.MapperStart(unbardir + $"/{filename}", HelperStaticFolder, prefix, bruteforce);
                                    }

                                    ZipFile.CreateFromDirectory(unbardir + $"/{filename}", tempdir + $"/{filename}_Mapped.zip");

                                    output = (File.ReadAllBytes(tempdir + $"/{filename}_Mapped.zip"), $"{filename}_Mapped.zip");
                                }
                                else
                                {
                                    if (subfolder == "on")
                                    {
                                        foreach (var dircursor in Directory.GetDirectories(unbardir))
                                        {
                                            int fileCount = Directory.GetFiles(dircursor).Length;

                                            if (fileCount > 0)
                                                await map.MapperStart(dircursor, HelperStaticFolder, prefix, bruteforce);
                                        }
                                    }
                                    else
                                    {
                                        int fileCount = Directory.GetFiles(unbardir).Length;

                                        if (fileCount > 0)
                                            await map.MapperStart(unbardir, HelperStaticFolder, prefix, bruteforce);
                                    }

                                    ZipFile.CreateFromDirectory(unbardir, tempdir + $"/{filename}_Mapped.zip");

                                    output = (File.ReadAllBytes(tempdir + $"/{filename}_Mapped.zip"), $"{filename}_Mapped.zip");
                                }

                                map = null;

                                if (Directory.Exists(tempdir))
                                    Directory.Delete(tempdir, true);

                                i++;
                                filedata.Flush();
                            }
                        }
                        ms.Flush();
                    }
                }

                if (Directory.Exists(maindir))
                    Directory.Delete(maindir, true);
            }

            return output;
        }

        public static (byte[]?, string)? CDS(Stream? PostData, string? ContentType)
        {
            (byte[]?, string)? output = null;

            if (PostData != null && !string.IsNullOrEmpty(ContentType))
            {
                string maindir = Directory.GetCurrentDirectory() + $"/static/HomeToolsCache/CDS_cache/{MiscUtils.GenerateDynamicCacheGuid(MiscUtils.GetCurrentDateTime())}";
                Directory.CreateDirectory(maindir);
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);
                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new())
                    {
                        PostData.CopyTo(ms);
                        ms.Position = 0;
                        int i = 0;
                        string filename = string.Empty;
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        string? sha1 = data.GetParameterValue("sha1");
                        foreach (var multipartfile in data.Files)
                        {
                            using (Stream filedata = multipartfile.Data)
                            {
                                filedata.Position = 0;

                                // Find the number of bytes in the stream
                                int contentLength = (int)filedata.Length;

                                // Create a byte array
                                byte[] buffer = new byte[contentLength];

                                // Read the contents of the memory stream into the byte array
                                filedata.Read(buffer, 0, contentLength);

                                filename = multipartfile.FileName;

                                string guid = MiscUtils.GenerateDynamicCacheGuid(filename);

                                string tempdir = $"{maindir}/{guid}";

                                if (!string.IsNullOrEmpty(sha1) && sha1.Length < 16)
                                    LoggerAccessor.LogWarn($"[HomeTools] - CDSProcess - Invalid SHA1 given via interface.");
                                else
                                {
                                    if (!string.IsNullOrEmpty(sha1))
                                    {
                                        sha1 = new MiscUtils().ExtractFirst16Characters(sha1);

                                        if (!string.IsNullOrEmpty(sha1))
                                        {
                                            Directory.CreateDirectory(tempdir);

                                            // We identify how input is like.

                                            byte[]? ProcessedFileBytes = CDSProcess.CDSEncrypt_Decrypt(buffer, sha1);

                                            if (ProcessedFileBytes != null)
                                            {
                                                if (buffer.Length > 4 && (buffer[0] == 0x3c && buffer[1] == 0x78 && buffer[2] == 0x6d && buffer[3] == 0x6c
                                                    || buffer[0] == 0x3c && buffer[1] == 0x58 && buffer[2] == 0x4d && buffer[3] == 0x4c))
                                                {
                                                    if (filename.ToLower().Contains(".sdc"))
                                                    {
                                                        File.WriteAllBytes(tempdir + $"/{filename}_Encrypted.sdc", ProcessedFileBytes);

                                                        output = (File.ReadAllBytes(tempdir + $"/{filename}_Encrypted.sdc"), $"{filename}_Encrypted.sdc");
                                                    }
                                                    else if (filename.ToLower().Contains(".odc"))
                                                    {
                                                        File.WriteAllBytes(tempdir + $"/{filename}_Encrypted.odc", ProcessedFileBytes);

                                                        output = (File.ReadAllBytes(tempdir + $"/{filename}_Encrypted.odc"), $"{filename}_Encrypted.odc");
                                                    }
                                                    else
                                                    {
                                                        File.WriteAllBytes(tempdir + $"/{filename}_Processed.xml", ProcessedFileBytes);

                                                        output = (File.ReadAllBytes(tempdir + $"/{filename}_Processed.xml"), $"{filename}_Processed.xml");
                                                    }
                                                }
                                                else if (buffer.Length > 4 && buffer[0] == 0x73 && buffer[1] == 0x65 && buffer[2] == 0x67 && buffer[3] == 0x73)
                                                {
                                                    File.WriteAllBytes(tempdir + $"/{filename}_Encrypted.hcdb", ProcessedFileBytes);

                                                    output = (File.ReadAllBytes(tempdir + $"/{filename}_Encrypted.hcdb"), $"{filename}_Encrypted.hcdb");
                                                }
                                                else if (buffer.Length > 4 && buffer[0] == 0xAD && buffer[1] == 0xEF && buffer[2] == 0x17 && buffer[3] == 0xE1)
                                                {
                                                    File.WriteAllBytes(tempdir + $"/{filename}_Encrypted.sharc", ProcessedFileBytes);

                                                    output = (File.ReadAllBytes(tempdir + $"/{filename}_Encrypted.sharc"), $"{filename}_Encrypted.sharc");
                                                }
                                                else if (buffer.Length > 4 && buffer[0] == 0xE1 && buffer[1] == 0x17 && buffer[2] == 0xEF && buffer[3] == 0xAD)
                                                {
                                                    File.WriteAllBytes(tempdir + $"/{filename}_Encrypted.bar", ProcessedFileBytes);

                                                    output = (File.ReadAllBytes(tempdir + $"/{filename}_Encrypted.bar"), $"{filename}_Encrypted.bar");
                                                }
                                                else
                                                {
                                                    if (ProcessedFileBytes.Length > 4 && (ProcessedFileBytes[0] == 0x3c && ProcessedFileBytes[1] == 0x78 && ProcessedFileBytes[2] == 0x6d && ProcessedFileBytes[3] == 0x6c
                                                    || ProcessedFileBytes[0] == 0x3c && ProcessedFileBytes[1] == 0x58 && ProcessedFileBytes[2] == 0x4d && ProcessedFileBytes[3] == 0x4c))
                                                    {
                                                        if (filename.ToLower().Contains(".sdc"))
                                                        {
                                                            File.WriteAllBytes(tempdir + $"/{filename}_Decrypted.sdc", ProcessedFileBytes);

                                                            output = (File.ReadAllBytes(tempdir + $"/{filename}_Decrypted.sdc"), $"{filename}_Decrypted.sdc");
                                                        }
                                                        else if (filename.ToLower().Contains(".odc"))
                                                        {
                                                            File.WriteAllBytes(tempdir + $"/{filename}_Decrypted.odc", ProcessedFileBytes);

                                                            output = (File.ReadAllBytes(tempdir + $"/{filename}_Decrypted.odc"), $"{filename}_Decrypted.odc");
                                                        }
                                                        else
                                                        {
                                                            File.WriteAllBytes(tempdir + $"/{filename}_Decrypted.xml", ProcessedFileBytes);

                                                            output = (File.ReadAllBytes(tempdir + $"/{filename}_Decrypted.xml"), $"{filename}_Decrypted.xml");
                                                        }
                                                    }
                                                    else if (ProcessedFileBytes.Length > 4 && ProcessedFileBytes[0] == 0x73 && ProcessedFileBytes[1] == 0x65 && ProcessedFileBytes[2] == 0x67 && ProcessedFileBytes[3] == 0x73)
                                                    {
                                                        byte[]? DecompressedData = new EDGELZMA().Decompress(ProcessedFileBytes, true);
                                                        if (DecompressedData != null)
                                                        {
                                                            File.WriteAllBytes(tempdir + $"/{filename}_Decrypted.sql", DecompressedData);
                                                            output = (File.ReadAllBytes(tempdir + $"/{filename}_Decrypted.sql"), $"{filename}_Decrypted.sql");
                                                        }
                                                        else
                                                        {
                                                            File.WriteAllBytes(tempdir + $"/{filename}_Decrypted.hcdb", ProcessedFileBytes);
                                                            output = (File.ReadAllBytes(tempdir + $"/{filename}_Decrypted.hcdb"), $"{filename}_Decrypted.hcdb");
                                                        }
                                                    }
                                                    else if (ProcessedFileBytes.Length > 4 && ProcessedFileBytes[0] == 0xAD && ProcessedFileBytes[1] == 0xEF && ProcessedFileBytes[2] == 0x17 && ProcessedFileBytes[3] == 0xE1)
                                                    {
                                                        File.WriteAllBytes(tempdir + $"/{filename}_Decrypted.sharc", ProcessedFileBytes);

                                                        output = (File.ReadAllBytes(tempdir + $"/{filename}_Decrypted.sharc"), $"{filename}_Decrypted.sharc");
                                                    }
                                                    else if (ProcessedFileBytes.Length > 4 && ProcessedFileBytes[0] == 0xE1 && ProcessedFileBytes[1] == 0x17 && ProcessedFileBytes[2] == 0xEF && ProcessedFileBytes[3] == 0xAD)
                                                    {
                                                        File.WriteAllBytes(tempdir + $"/{filename}_Decrypted.bar", ProcessedFileBytes);

                                                        output = (File.ReadAllBytes(tempdir + $"/{filename}_Decrypted.bar"), $"{filename}_Decrypted.bar");
                                                    }
                                                    else // If all scan failed, fallback.
                                                    {
                                                        File.WriteAllBytes(tempdir + $"/{filename}_Processed.bin", ProcessedFileBytes);

                                                        output = (File.ReadAllBytes(tempdir + $"/{filename}_Processed.bin"), $"{filename}_Processed.bin");
                                                    }
                                                }
                                            }

                                            if (Directory.Exists(tempdir))
                                                Directory.Delete(tempdir, true);
                                        }
                                    }
                                }

                                i++;
                                filedata.Flush();
                            }
                        }
                        ms.Flush();
                    }
                }

                if (Directory.Exists(maindir))
                    Directory.Delete(maindir, true);
            }

            return output;
        }

        public static (byte[]?, string)? TicketList(Stream? PostData, string? ContentType)
        {
            (byte[]?, string)? output = null;

            if (PostData != null && !string.IsNullOrEmpty(ContentType))
            {
                string maindir = Directory.GetCurrentDirectory() + $"/static/HomeToolsCache/TicketList_cache/{MiscUtils.GenerateDynamicCacheGuid(MiscUtils.GetCurrentDateTime())}";
                Directory.CreateDirectory(maindir);
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);
                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new())
                    {
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

                        }
                        foreach (var multipartfile in data.Files)
                        {
                            using (Stream filedata = multipartfile.Data)
                            {
                                filedata.Position = 0;

                                // Find the number of bytes in the stream
                                int contentLength = (int)filedata.Length;

                                // Create a byte array
                                byte[] buffer = new byte[contentLength];

                                // Read the contents of the memory stream into the byte array
                                filedata.Read(buffer, 0, contentLength);

                                filename = multipartfile.FileName;

                                string guid = MiscUtils.GenerateDynamicCacheGuid(filename);

                                string tempdir = $"{maindir}/{guid}";

                                Directory.CreateDirectory(tempdir);

                                if (buffer.Length > 8 && buffer[0] == 0xBE && buffer[1] == 0xE5 && buffer[2] == 0xBE && buffer[3] == 0xE5
                                     && buffer[4] == 0x00 && buffer[5] == 0x00 && buffer[6] == 0x00 && buffer[7] == 0x01 && version1 == "on")
                                {
                                    byte[]? ProcessedFileBytes = new byte[buffer.Length - 8];
                                    Buffer.BlockCopy(buffer, 8, ProcessedFileBytes, 0, ProcessedFileBytes.Length);
                                    ProcessedFileBytes = new BlowfishCTREncryptDecrypt().TicketListV1Process(ProcessedFileBytes);

                                    File.WriteAllBytes(tempdir + $"/{filename}_Decrypted.lst", ProcessedFileBytes);

                                    output = (File.ReadAllBytes(tempdir + $"/{filename}_Decrypted.lst"), $"{filename}_Decrypted.lst");
                                }
                                else if (version1 == "on")
                                {
                                    byte[] ProcessedFileBytes = new MiscUtils().Combinebytearay(new byte[] { 0xBE, 0xE5, 0xBE, 0xE5, 0x00, 0x00, 0x00, 0x01 }, new BlowfishCTREncryptDecrypt().TicketListV1Process(buffer));

                                    File.WriteAllBytes(tempdir + $"/{filename}_Encrypted.lst", ProcessedFileBytes);

                                    output = (File.ReadAllBytes(tempdir + $"/{filename}_Encrypted.lst"), $"{filename}_Encrypted.lst");
                                }
                                else if (buffer.Length > 8 && buffer[0] == 0xBE && buffer[1] == 0xE5 && buffer[2] == 0xBE && buffer[3] == 0xE5
                                    && buffer[4] == 0x00 && buffer[5] == 0x00 && buffer[6] == 0x00 && buffer[7] == 0x00)
                                {
                                    byte[]? ProcessedFileBytes = new byte[buffer.Length - 8];
                                    Buffer.BlockCopy(buffer, 8, ProcessedFileBytes, 0, ProcessedFileBytes.Length);
                                    ProcessedFileBytes = new BlowfishCTREncryptDecrypt().TicketListV0Process(ProcessedFileBytes);

                                    File.WriteAllBytes(tempdir + $"/{filename}_Decrypted.lst", ProcessedFileBytes);

                                    output = (File.ReadAllBytes(tempdir + $"/{filename}_Decrypted.lst"), $"{filename}_Decrypted.lst");
                                }
                                else
                                {
                                    byte[] ProcessedFileBytes = new MiscUtils().Combinebytearay(new byte[] { 0xBE, 0xE5, 0xBE, 0xE5, 0x00, 0x00, 0x00, 0x00 }, new BlowfishCTREncryptDecrypt().TicketListV0Process(buffer));

                                    File.WriteAllBytes(tempdir + $"/{filename}_Encrypted.lst", ProcessedFileBytes);

                                    output = (File.ReadAllBytes(tempdir + $"/{filename}_Encrypted.lst"), $"{filename}_Encrypted.lst");
                                }

                                if (Directory.Exists(tempdir))
                                    Directory.Delete(tempdir, true);

                                i++;
                                filedata.Flush();
                            }
                        }
                        ms.Flush();
                    }
                }

                if (Directory.Exists(maindir))
                    Directory.Delete(maindir, true);
            }

            return output;
        }

        public static (byte[]?, string)? INF(Stream? PostData, string? ContentType)
        {
            (byte[]?, string)? output = null;

            if (PostData != null && !string.IsNullOrEmpty(ContentType))
            {
                string maindir = Directory.GetCurrentDirectory() + $"/static/HomeToolsCache/INF_cache/{MiscUtils.GenerateDynamicCacheGuid(MiscUtils.GetCurrentDateTime())}";
                Directory.CreateDirectory(maindir);
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);
                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new())
                    {
                        PostData.CopyTo(ms);
                        ms.Position = 0;
                        int i = 0;
                        string filename = string.Empty;
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        foreach (var multipartfile in data.Files)
                        {
                            using (Stream filedata = multipartfile.Data)
                            {
                                filedata.Position = 0;

                                // Find the number of bytes in the stream
                                int contentLength = (int)filedata.Length;

                                // Create a byte array
                                byte[] buffer = new byte[contentLength];

                                // Read the contents of the memory stream into the byte array
                                filedata.Read(buffer, 0, contentLength);

                                filename = multipartfile.FileName;

                                string guid = MiscUtils.GenerateDynamicCacheGuid(filename);

                                string tempdir = $"{maindir}/{guid}";

                                if (buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0x00 && buffer[3] == 0x01 && ToolsImpl.INFIVA != null)
                                {
                                    Directory.CreateDirectory(tempdir);

                                    ToolsImpl? toolsimpl = new();

                                    buffer = toolsimpl.RemovePaddingPrefix(buffer);

                                    byte[]? decryptedfilebytes = toolsimpl.Crypt_Decrypt(buffer, ToolsImpl.INFIVA, 8);

                                    toolsimpl = null;

                                    if (decryptedfilebytes != null)
                                    {
                                        File.WriteAllBytes(tempdir + $"/{filename}_Processed.bin", decryptedfilebytes);

                                        output = (File.ReadAllBytes(tempdir + $"/{filename}_Processed.bin"), $"{filename}_Processed.bin");
                                    }

                                    if (Directory.Exists(tempdir))
                                        Directory.Delete(tempdir, true);
                                }
                                else if (buffer[0] == 0xBE && buffer[1] == 0xE5 && buffer[2] == 0xBE && buffer[3] == 0xE5 && ToolsImpl.INFIVA != null)
                                {
                                    Directory.CreateDirectory(tempdir);

                                    ToolsImpl? toolsimpl = new();

                                    byte[]? encryptedfilebytes = toolsimpl.Crypt_Decrypt(buffer, ToolsImpl.INFIVA, 8);

                                    if (encryptedfilebytes != null)
                                    {
                                        File.WriteAllBytes(tempdir + $"/{filename}_Processed.bin", toolsimpl.ApplyPaddingPrefix(encryptedfilebytes));

                                        output = (File.ReadAllBytes(tempdir + $"/{filename}_Processed.bin"), $"{filename}_Processed.bin");
                                    }

                                    toolsimpl = null;

                                    if (Directory.Exists(tempdir))
                                        Directory.Delete(tempdir, true);
                                }

                                i++;
                                filedata.Flush();
                            }
                        }
                        ms.Flush();
                    }
                }

                if (Directory.Exists(maindir))
                    Directory.Delete(maindir, true);
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
                    using (MemoryStream ms = new())
                    {
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

                        }
                        try
                        {
                            newerhome = data.GetParameterValue("newerhome");
                        }
                        catch (Exception)
                        {

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
                    using (MemoryStream ms = new())
                    {
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
                                res = "Invalid ChannelID or unsupported format";
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
                                res = "Invalid ChannelID or unsupported format";
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
            }

            return res;
        }

        public static void UncompressFile(string compressedFilePath, string extractionFolderPath)
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
