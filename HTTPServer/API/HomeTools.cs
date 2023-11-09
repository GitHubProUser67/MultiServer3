using CryptoSporidium;
using CryptoSporidium.BAR;
using CryptoSporidium.ChannelID;
using CryptoSporidium.FileHelper;
using CryptoSporidium.UnBAR;
using CustomLogger;
using HttpMultipartParser;
using System.IO.Compression;
using System.Net;

namespace HTTPServer.API
{
    public class HomeTools
    {
        public static bool MakeBarSdat(Stream? PostData, string? ContentType, HttpListenerResponse response)
        {
            bool isok = false;

            if (PostData != null && !string.IsNullOrEmpty(ContentType))
            {
                string maindir = $"{HTTPServerConfiguration.HTTPStaticFolder}/HomeTools/MakeBarSdat_cache/{Misc.GenerateDynamicCacheGuid(Misc.GetCurrentDateTime())}";
                Directory.CreateDirectory(maindir);
                string? boundary = CryptoSporidium.HTTPUtils.ExtractBoundary(ContentType);
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
                        string options = data.GetParameterValue("options");
                        string version2 = string.Empty;
                        string encrypt = string.Empty;
                        try
                        {
                            version2 = data.GetParameterValue("version2");
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

                                string guid = Misc.GenerateDynamicCacheGuid(filename);

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
                                if (version2 == "on")
                                {
                                    if (encrypt == "on")
                                        bararchive = new BARArchive(string.Format("{0}/{1}.SHARC", rebardir, filename), unzipdir, true, true);
                                    else
                                        bararchive = new BARArchive(string.Format("{0}/{1}.SHARC", rebardir, filename), unzipdir, true);
                                }
                                else
                                {
                                    if (encrypt == "on")
                                        bararchive = new BARArchive(string.Format("{0}/{1}.BAR", rebardir, filename), unzipdir, false, true);
                                    else
                                        bararchive = new BARArchive(string.Format("{0}/{1}.BAR", rebardir, filename), unzipdir);
                                }
                                bararchive.BARHeader.UserData = Convert.ToInt32(TimeStamp, 16);
                                bararchive.DefaultCompression = CompressionMethod.EdgeZLib;
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

                                bararchive.EndUpdate();
                                bararchive.Save();

                                if (version2 == "on")
                                {
                                    AESCTR256EncryptDecrypt? aes256 = new();
                                    ToolsImpl? toolsImpl = new();
                                    MiscUtils? utils = new();
                                    // Read the original data from the file
                                    byte[] BarData = File.ReadAllBytes(rebardir + $"/{filename}.SHARC");
                                    byte[]? HeaderIV = RunUnBAR.ExtractSHARCHeaderIV(BarData);
                                    if (HeaderIV != null)
                                    {
                                        byte[]? Header = RunUnBAR.ExtractEncryptedSharcHeaderData(BarData);

                                        if (Header != null)
                                        {
                                            byte[]? EncryptedHeader = aes256.InitiateCTRBuffer(Header,
                                                    Convert.FromBase64String(options), HeaderIV);

                                            if (EncryptedHeader != null)
                                            {
                                                toolsImpl.IncrementIVBytes(HeaderIV, 1); // IV so we increment.
                                                byte[]? NumOfFiles = RunUnBAR.ExtractNumOfFiles(Header);
                                                if (NumOfFiles != null)
                                                {
                                                    uint TOCSize = 24 * BitConverter.ToUInt32(NumOfFiles, 0);
                                                    byte[]? DecryptedTOC = utils.CopyBytes(utils.TrimStart(BarData, 52), TOCSize);
                                                    if (DecryptedTOC != null)
                                                    {
                                                        byte[]? EncryptedTOC = aes256.InitiateCTRBuffer(utils.CopyBytes(utils.TrimStart(BarData, 52), TOCSize), Convert.FromBase64String(options), HeaderIV);

                                                        if (EncryptedTOC != null)
                                                        {
                                                            byte[] FileBytes = utils.Combinebytearay(new byte[] { 0xAD, 0xEF, 0x17, 0xE1, 0x02, 0x00, 0x00, 0x00 },
                                                                utils.Combinebytearay(bararchive.BARHeader.IV, utils.Combinebytearay(EncryptedHeader,
                                                                utils.Combinebytearay(EncryptedTOC, utils.TrimBytes(utils.TrimStart(BarData, 52), TOCSize)))));

                                                            // Write the patched data back to the file
                                                            File.WriteAllBytes(rebardir + $"/{filename}.SHARC", FileBytes);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    aes256 = null;
                                    toolsImpl = null;
                                    utils = null;
                                }

                                bararchive = null;

                                FileHelper filehelper = new();

                                if (mode == "bar")
                                {
                                    using (FileStream zipStream = new FileStream(rebardir + $"/{filename}_Rebar.zip", FileMode.Create))
                                    {
                                        if (version2 == "on")
                                        {
                                            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                                            {
                                                // Add the first file to the archive
                                                ZipArchiveEntry entry1 = archive.CreateEntry($"{filename}.SHARC");
                                                using (Stream entryStream = entry1.Open())
                                                {
                                                    using (FileStream fileStream = new FileStream(rebardir + $"/{filename}.SHARC", FileMode.Open))
                                                    {
                                                        fileStream.CopyTo(entryStream);
                                                    }
                                                }

                                                // Add the second file to the archive
                                                ZipArchiveEntry entry2 = archive.CreateEntry($"{filename}.sharc.map");
                                                using (Stream entryStream = entry2.Open())
                                                {
                                                    using (FileStream fileStream = new FileStream(rebardir + $"/{filename}.sharc.map", FileMode.Open))
                                                    {
                                                        fileStream.CopyTo(entryStream);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                                            {
                                                // Add the first file to the archive
                                                ZipArchiveEntry entry1 = archive.CreateEntry($"{filename}.BAR");
                                                using (Stream entryStream = entry1.Open())
                                                {
                                                    using (FileStream fileStream = new FileStream(rebardir + $"/{filename}.BAR", FileMode.Open))
                                                    {
                                                        fileStream.CopyTo(entryStream);
                                                    }
                                                }

                                                // Add the second file to the archive
                                                ZipArchiveEntry entry2 = archive.CreateEntry($"{filename}.bar.map");
                                                using (Stream entryStream = entry2.Open())
                                                {
                                                    using (FileStream fileStream = new FileStream(rebardir + $"/{filename}.bar.map", FileMode.Open))
                                                    {
                                                        fileStream.CopyTo(entryStream);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    HTTPResponseWriteFile(response, rebardir + $"/{filename}_Rebar.zip");

                                    isok = true;
                                }
                                else if (mode == "sdatnpd" && File.Exists(Directory.GetCurrentDirectory() + "/static/model.sdat"))
                                {
                                    RunUnBAR? unbar = new();
                                    if (version2 == "on")
                                        unbar.RunEncrypt(rebardir + $"/{filename}.SHARC", rebardir + $"/{filename.ToLower()}.sdat", Directory.GetCurrentDirectory() + "/static/model.sdat");
                                    else
                                        unbar.RunEncrypt(rebardir + $"/{filename}.BAR", rebardir + $"/{filename.ToLower()}.sdat", Directory.GetCurrentDirectory() + "/static/model.sdat");
                                    unbar = null;

                                    using (FileStream zipStream = new FileStream(rebardir + $"/{filename}_Rebar.zip", FileMode.Create))
                                    {
                                        using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                                        {
                                            // Add the first file to the archive
                                            ZipArchiveEntry entry1 = archive.CreateEntry($"{filename.ToLower()}.sdat");
                                            using (Stream entryStream = entry1.Open())
                                            {
                                                using (FileStream fileStream = new FileStream(rebardir + $"/{filename.ToLower()}.sdat", FileMode.Open))
                                                {
                                                    fileStream.CopyTo(entryStream);
                                                }
                                            }

                                            if (version2 == "on")
                                            {
                                                // Add the second file to the archive
                                                ZipArchiveEntry entry2 = archive.CreateEntry($"{filename}.sharc.map");
                                                using (Stream entryStream = entry2.Open())
                                                {
                                                    using (FileStream fileStream = new FileStream(rebardir + $"/{filename}.sharc.map", FileMode.Open))
                                                    {
                                                        fileStream.CopyTo(entryStream);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // Add the second file to the archive
                                                ZipArchiveEntry entry2 = archive.CreateEntry($"{filename}.bar.map");
                                                using (Stream entryStream = entry2.Open())
                                                {
                                                    using (FileStream fileStream = new FileStream(rebardir + $"/{filename}.bar.map", FileMode.Open))
                                                    {
                                                        fileStream.CopyTo(entryStream);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    HTTPResponseWriteFile(response, rebardir + $"/{filename}_Rebar.zip");

                                    isok = true;
                                }
                                else
                                {
                                    RunUnBAR? unbar = new();
                                    if (version2 == "on")
                                        unbar.RunEncrypt(rebardir + $"/{filename}.SHARC", rebardir + $"/{filename.ToLower()}.sdat", null);
                                    else
                                        unbar.RunEncrypt(rebardir + $"/{filename}.BAR", rebardir + $"/{filename.ToLower()}.sdat", null);
                                    unbar = null;

                                    using (FileStream zipStream = new FileStream(rebardir + $"/{filename}_Rebar.zip", FileMode.Create))
                                    {
                                        using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                                        {
                                            // Add the first file to the archive
                                            ZipArchiveEntry entry1 = archive.CreateEntry($"{filename.ToLower()}.sdat");
                                            using (Stream entryStream = entry1.Open())
                                            {
                                                using (FileStream fileStream = new FileStream(rebardir + $"/{filename.ToLower()}.sdat", FileMode.Open))
                                                {
                                                    fileStream.CopyTo(entryStream);
                                                }
                                            }

                                            if (version2 == "on")
                                            {
                                                // Add the second file to the archive
                                                ZipArchiveEntry entry2 = archive.CreateEntry($"{filename}.sharc.map");
                                                using (Stream entryStream = entry2.Open())
                                                {
                                                    using (FileStream fileStream = new FileStream(rebardir + $"/{filename}.sharc.map", FileMode.Open))
                                                    {
                                                        fileStream.CopyTo(entryStream);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // Add the second file to the archive
                                                ZipArchiveEntry entry2 = archive.CreateEntry($"{filename}.bar.map");
                                                using (Stream entryStream = entry2.Open())
                                                {
                                                    using (FileStream fileStream = new FileStream(rebardir + $"/{filename}.bar.map", FileMode.Open))
                                                    {
                                                        fileStream.CopyTo(entryStream);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    HTTPResponseWriteFile(response, rebardir + $"/{filename}_Rebar.zip");

                                    isok = true;
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

            return isok;
        }

        public static async Task<bool> UnBar(Stream? PostData, string? ContentType, HttpListenerResponse response)
        {
            bool isok = false;

            if (PostData != null && !string.IsNullOrEmpty(ContentType))
            {
                string maindir = $"{HTTPServerConfiguration.HTTPStaticFolder}/HomeTools/UnBar_cache/{Misc.GenerateDynamicCacheGuid(Misc.GetCurrentDateTime())}";
                Directory.CreateDirectory(maindir);
                string? boundary = CryptoSporidium.HTTPUtils.ExtractBoundary(ContentType);
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

                                string guid = Misc.GenerateDynamicCacheGuid(filename);

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
                                                await map.MapperStart(dircursor, HTTPServerConfiguration.HomeToolsHelperStaticFolder, prefix, bruteforce);
                                        }
                                    }
                                    else
                                    {
                                        int fileCount = Directory.GetFiles(unbardir + $"/{filename}").Length;

                                        if (fileCount > 0)
                                            await map.MapperStart(unbardir + $"/{filename}", HTTPServerConfiguration.HomeToolsHelperStaticFolder, prefix, bruteforce);
                                    }


                                    ZipFile.CreateFromDirectory(unbardir + $"/{filename}", tempdir + $"/{filename}_Mapped.zip");

                                    HTTPResponseWriteFile(response, tempdir + $"/{filename}_Mapped.zip");

                                    isok = true;
                                }
                                else if (Directory.Exists(unbardir + $"/{filename}"))
                                {
                                    if (subfolder == "on")
                                    {
                                        foreach (var dircursor in Directory.GetDirectories(unbardir + $"/{filename}"))
                                        {
                                            int fileCount = Directory.GetFiles(dircursor).Length;

                                            if (fileCount > 0)
                                                await map.MapperStart(dircursor, HTTPServerConfiguration.HomeToolsHelperStaticFolder, prefix, bruteforce);
                                        }
                                    }
                                    else
                                    {
                                        int fileCount = Directory.GetFiles(unbardir + $"/{filename}").Length;

                                        if (fileCount > 0)
                                            await map.MapperStart(unbardir + $"/{filename}", HTTPServerConfiguration.HomeToolsHelperStaticFolder, prefix, bruteforce);
                                    }

                                    ZipFile.CreateFromDirectory(unbardir + $"/{filename}", tempdir + $"/{filename}_Mapped.zip");

                                    HTTPResponseWriteFile(response, tempdir + $"/{filename}_Mapped.zip");

                                    isok = true;
                                }
                                else
                                {
                                    if (subfolder == "on")
                                    {
                                        foreach (var dircursor in Directory.GetDirectories(unbardir))
                                        {
                                            int fileCount = Directory.GetFiles(dircursor).Length;

                                            if (fileCount > 0)
                                                await map.MapperStart(dircursor, HTTPServerConfiguration.HomeToolsHelperStaticFolder, prefix, bruteforce);
                                        }
                                    }
                                    else
                                    {
                                        int fileCount = Directory.GetFiles(unbardir).Length;

                                        if (fileCount > 0)
                                            await map.MapperStart(unbardir, HTTPServerConfiguration.HomeToolsHelperStaticFolder, prefix, bruteforce);
                                    }

                                    ZipFile.CreateFromDirectory(unbardir, tempdir + $"/{filename}_Mapped.zip");

                                    HTTPResponseWriteFile(response, tempdir + $"/{filename}_Mapped.zip");

                                    isok = true;
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

            return isok;
        }

        public static bool CDS(Stream? PostData, string? ContentType, HttpListenerResponse response)
        {
            bool isok = false;

            if (PostData != null && !string.IsNullOrEmpty(ContentType))
            {
                string maindir = $"{HTTPServerConfiguration.HTTPStaticFolder}/HomeTools/CDS_cache/{Misc.GenerateDynamicCacheGuid(Misc.GetCurrentDateTime())}";
                Directory.CreateDirectory(maindir);
                string? boundary = CryptoSporidium.HTTPUtils.ExtractBoundary(ContentType);
                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new())
                    {
                        PostData.CopyTo(ms);
                        ms.Position = 0;
                        int i = 0;
                        string filename = string.Empty;
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        string sha1 = data.GetParameterValue("sha1");
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

                                string guid = Misc.GenerateDynamicCacheGuid(filename);

                                string tempdir = $"{maindir}/{guid}";

                                if (sha1.Length < 16)
                                    LoggerAccessor.LogWarn($"[HomeTools] - CDSProcess - Invalid SHA1 given via interface.");
                                else
                                {
                                    Directory.CreateDirectory(tempdir);

                                    byte[]? ProcessedFileBytes = CryptoSporidium.CDS.CDSProcess.CDSEncrypt_Decrypt(buffer, sha1);

                                    if (ProcessedFileBytes != null)
                                    {
                                        if (filename.ToLower().Contains(".xml") || filename.ToLower().Contains(".sdc") || filename.ToLower().Contains(".odc"))
                                        {
                                            File.WriteAllBytes(tempdir + $"/{filename}_Processed.xml", ProcessedFileBytes);

                                            HTTPResponseWriteFile(response, tempdir + $"/{filename}_Processed.xml");

                                            isok = true;
                                        }
                                        else
                                        {
                                            File.WriteAllBytes(tempdir + $"/{filename}_Processed.bin", ProcessedFileBytes);

                                            HTTPResponseWriteFile(response, tempdir + $"/{filename}_Processed.bin");

                                            isok = true;
                                        }
                                    }

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

            return isok;
        }

        public static bool INF(Stream? PostData, string? ContentType, HttpListenerResponse response)
        {
            bool isok = false;

            if (PostData != null && !string.IsNullOrEmpty(ContentType))
            {
                string maindir = $"{HTTPServerConfiguration.HTTPStaticFolder}/HomeTools/INF_cache/{Misc.GenerateDynamicCacheGuid(Misc.GetCurrentDateTime())}";
                Directory.CreateDirectory(maindir);
                string? boundary = CryptoSporidium.HTTPUtils.ExtractBoundary(ContentType);
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

                                string guid = Misc.GenerateDynamicCacheGuid(filename);

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

                                        HTTPResponseWriteFile(response, tempdir + $"/{filename}_Processed.bin");

                                        isok = true;
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

                                        HTTPResponseWriteFile(response, tempdir + $"/{filename}_Processed.bin");

                                        isok = true;
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

            return isok;
        }

        public static string? ChannelID(Stream? PostData, string? ContentType)
        {
            string? res = null;

            if (PostData != null && !string.IsNullOrEmpty(ContentType))
            {
                string? boundary = CryptoSporidium.HTTPUtils.ExtractBoundary(ContentType);
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
                string? boundary = CryptoSporidium.HTTPUtils.ExtractBoundary(ContentType);
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

        public static void HTTPResponseWriteFile(HttpListenerResponse response, string path)
        {
            using (FileStream fs = File.OpenRead(path))
            {
                string filename = Path.GetFileName(path);
                response.AddHeader("Content-disposition", "attachment; filename=" + filename);
                byte[] buffer = new byte[64 * 1024];
                int read = 0;
                if (response.OutputStream.CanWrite)
                {
                    using (BinaryWriter bw = new(response.OutputStream))
                    {
                        while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            bw.Write(buffer, 0, read);
                            bw.Flush();
                        }

                        bw.Flush();
                    }
                }

                fs.Flush();
            }
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
