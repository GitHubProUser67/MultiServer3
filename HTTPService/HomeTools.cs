using HttpMultipartParser;
using System.Net;
using System.IO.Compression;
using MultiServer.CryptoSporidium.BAR;
using MultiServer.CryptoSporidium.UnBAR;
using System.Text;
using MultiServer.CryptoSporidium.ChannelID;
using MultiServer.CryptoSporidium;

namespace MultiServer.HTTPService
{
    public class HomeTools
    {
        public static async Task MakeBarSdat(HttpListenerRequest request, HttpListenerResponse response)
        {
            string maindir = Directory.GetCurrentDirectory() + $"/static/HomeTools/ReBar_cache/{Misc.generatedynamiccacheguid(Misc.GetCurrentDateTime())}";

            try
            {
                string boundary = Extensions.ExtractBoundary(request.ContentType);

                // Get the input stream from the context
                Stream inputStream = request.InputStream;

                // Create a memory stream to copy the content
                using (MemoryStream copyStream = new MemoryStream())
                {
                    // Copy the input stream to the memory stream
                    inputStream.CopyTo(copyStream);

                    // Reset the position of the copy stream to the beginning
                    copyStream.Position = 0;

                    var data = MultipartFormDataParser.Parse(copyStream, boundary);

                    string mode = data.GetParameterValue("mode");

                    string TimeStamp = data.GetParameterValue("TimeStamp");

                    string filename = "";

                    foreach (var multipartfile in data.Files)
                    {
                        Stream filedata = multipartfile.Data;

                        filedata.Position = 0;

                        // Find the number of bytes in the stream
                        int contentLength = (int)filedata.Length;

                        // Create a byte array
                        byte[] buffer = new byte[contentLength];

                        // Read the contents of the memory stream into the byte array
                        filedata.Read(buffer, 0, contentLength);

                        filename = multipartfile.FileName;

                        string guid = Misc.generatedynamiccacheguid(filename);

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
                        BARArchive bararchive = new BARArchive(string.Format("{0}/{1}.BAR", rebardir, filename), unzipdir);
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
                        StreamWriter writer = new StreamWriter(unzipdir + @"/files.txt");

                        // Get all files in the directory and its immediate subdirectories
                        string[] files = Directory.GetFiles(unzipdir, "*.*", SearchOption.AllDirectories);

                        // Loop through the files and write their paths to the text file
                        foreach (string file in files)
                        {
                            string relativePath = "file=\"" + file.Replace(unzipdir + @"/", "") + "\"";
                            writer.WriteLine(relativePath.Replace(@"\", "/"));
                        }

                        writer.Close();

                        bararchive.AddFile(unzipdir + @"/files.txt");

                        bararchive.EndUpdate();
                        bararchive.Save();

                        // Read the original data from the file
                        byte[] BarData = File.ReadAllBytes(rebardir + $"/{filename}.BAR");

                        // Patch the value at the specified offset (0x0C to 0x0F)
                        int offset = 0x0C;
                        byte[] newValueBytes = BitConverter.GetBytes(Convert.ToInt32(TimeStamp, 16));
                        Array.Copy(newValueBytes, 0, BarData, offset, 4); // Patch the value

                        // Write the patched data back to the file
                        File.WriteAllBytes(rebardir + $"/{filename}.BAR", BarData);

                        if (mode == "bar")
                        {
                            using (FileStream zipStream = new FileStream(rebardir + $"/{filename}_Rebar.zip", FileMode.Create))
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

                            await FileHelper.HTTPResponseWriteFile(response, rebardir + $"/{filename}_Rebar.zip");

                            response.StatusCode = (int)HttpStatusCode.OK;
                        }
                        else if (mode == "sdatnpd" && File.Exists(Directory.GetCurrentDirectory() + "/static/model.sdat"))
                        {
                            RunUnBAR.RunEncrypt(rebardir + $"/{filename}.BAR", rebardir + $"/{filename.ToLower()}.sdat", Directory.GetCurrentDirectory() + "/static/model.sdat");

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

                            await FileHelper.HTTPResponseWriteFile(response, rebardir + $"/{filename}_Rebar.zip");
                        }
                        else
                        {
                            RunUnBAR.RunEncrypt(rebardir + $"/{filename}.BAR", rebardir + $"/{filename.ToLower()}.sdat", null);

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

                            await FileHelper.HTTPResponseWriteFile(response, rebardir + $"/{filename}_Rebar.zip");
                        }

                        if (Directory.Exists(tempdir))
                            Directory.Delete(tempdir, true);
                    }

                    copyStream.Dispose();
                }

                response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 64) // Can happen if spammed.
            {
                response.StatusCode = 500;
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[HomeTools] - MakeBarSdat Errored out with this exception : {ex}");
                response.StatusCode = 500;
            }

            if (Directory.Exists(maindir))
                Directory.Delete(maindir, true);
        }

        public static async Task UnBARProcess(HttpListenerRequest request, HttpListenerResponse response)
        {
            string maindir = Directory.GetCurrentDirectory() + $"/static/HomeTools/UnBar_cache/{Misc.generatedynamiccacheguid(Misc.GetCurrentDateTime())}";

            try
            {
                string boundary = Extensions.ExtractBoundary(request.ContentType);

                // Get the input stream from the context
                Stream inputStream = request.InputStream;

                // Create a memory stream to copy the content
                using (MemoryStream copyStream = new MemoryStream())
                {
                    // Copy the input stream to the memory stream
                    inputStream.CopyTo(copyStream);

                    // Reset the position of the copy stream to the beginning
                    copyStream.Position = 0;

                    var data = MultipartFormDataParser.Parse(copyStream, boundary);

                    string options = data.GetParameterValue("options");

                    string prefix = data.GetParameterValue("prefix");

                    string subfolder = "";

                    string bruteforce = "";

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

                    string filename = "";

                    string ogfilename = "";

                    if (options == "cdn1")
                        options = AFSMISC.base64CDNKey1;
                    else if (options == "cdn2")
                        options = AFSMISC.base64CDNKey2;
                    else
                        options = Convert.ToBase64String(AFSMISC.DefaultKey); // Not good probably. An other key seems in use.

                    foreach (var multipartfile in data.Files)
                    {
                        Stream filedata = multipartfile.Data;

                        filedata.Position = 0;

                        // Find the number of bytes in the stream
                        int contentLength = (int)filedata.Length;

                        // Create a byte array
                        byte[] buffer = new byte[contentLength];

                        // Read the contents of the memory stream into the byte array
                        filedata.Read(buffer, 0, contentLength);

                        filename = multipartfile.FileName;

                        string guid = Misc.generatedynamiccacheguid(filename);

                        string tempdir = $"{maindir}/{guid}";

                        string unbardir = tempdir + $"/unbar";

                        string barfile = tempdir + $"/{filename}";

                        Directory.CreateDirectory(unbardir);

                        File.WriteAllBytes(barfile, buffer);

                        if (filename.ToLower().EndsWith(".bar") || filename.ToLower().EndsWith(".dat"))
                        {
                            RunUnBAR.Run(barfile, unbardir, false, options);
                            ogfilename = filename;
                            filename = filename.Substring(0, filename.Length - 4).ToUpper();
                        }
                        else if (filename.ToLower().EndsWith(".sharc"))
                        {
                            RunUnBAR.Run(barfile, unbardir, false, options);
                            ogfilename = filename;
                            filename = filename.Substring(0, filename.Length - 4).ToUpper();
                        }
                        else if (filename.ToLower().EndsWith(".sdat"))
                        {
                            RunUnBAR.Run(barfile, unbardir, true, options);
                            ogfilename = filename;
                            filename = filename.Substring(0, filename.Length - 5).ToUpper();
                        }
                        else if (filename.ToLower().EndsWith(".zip"))
                        {
                            UncompressFile(barfile, unbardir);
                            ogfilename = filename;
                            filename = filename.Substring(0, filename.Length - 4).ToUpper();
                        }

                        Mapper map = new Mapper();

                        if (Directory.Exists(unbardir + $"/{filename}") && (ogfilename.ToLower().EndsWith(".bar") || ogfilename.ToLower().EndsWith(".sharc") || ogfilename.ToLower().EndsWith(".sdat")))
                        {
                            if (subfolder == "on")
                            {
                                foreach (var dircursor in Directory.GetDirectories(unbardir + $"/{filename}"))
                                {
                                    int fileCount = Directory.GetFiles(dircursor).Length;

                                    if (fileCount > 0)
                                        await map.MapperStart(dircursor, prefix, bruteforce);
                                }
                            }
                            else
                            {
                                int fileCount = Directory.GetFiles(unbardir + $"/{filename}").Length;

                                if (fileCount > 0)
                                    await map.MapperStart(unbardir + $"/{filename}", prefix, bruteforce);
                            }


                            ZipFile.CreateFromDirectory(unbardir + $"/{filename}", tempdir + $"/{filename}_Mapped.zip");

                            await FileHelper.HTTPResponseWriteFile(response, tempdir + $"/{filename}_Mapped.zip");
                        }
                        else if (Directory.Exists(unbardir + $"/{filename}"))
                        {
                            if (subfolder == "on")
                            {
                                foreach (var dircursor in Directory.GetDirectories(unbardir + $"/{filename}"))
                                {
                                    int fileCount = Directory.GetFiles(dircursor).Length;

                                    if (fileCount > 0)
                                        await map.MapperStart(dircursor, prefix, bruteforce);
                                }
                            }
                            else
                            {
                                int fileCount = Directory.GetFiles(unbardir + $"/{filename}").Length;

                                if (fileCount > 0)
                                    await map.MapperStart(unbardir + $"/{filename}", prefix, bruteforce);
                            }

                            ZipFile.CreateFromDirectory(unbardir + $"/{filename}", tempdir + $"/{filename}_Mapped.zip");

                            await FileHelper.HTTPResponseWriteFile(response, tempdir + $"/{filename}_Mapped.zip");
                        }
                        else
                        {
                            if (subfolder == "on")
                            {
                                foreach (var dircursor in Directory.GetDirectories(unbardir))
                                {
                                    int fileCount = Directory.GetFiles(dircursor).Length;

                                    if (fileCount > 0)
                                        await map.MapperStart(dircursor, prefix, bruteforce);
                                }
                            }
                            else
                            {
                                int fileCount = Directory.GetFiles(unbardir).Length;

                                if (fileCount > 0)
                                    await map.MapperStart(unbardir, prefix, bruteforce);
                            }

                            ZipFile.CreateFromDirectory(unbardir, tempdir + $"/{filename}_Mapped.zip");

                            await FileHelper.HTTPResponseWriteFile(response, tempdir + $"/{filename}_Mapped.zip");
                        }

                        if (Directory.Exists(tempdir))
                            Directory.Delete(tempdir, true);
                    }

                    copyStream.Dispose();
                }

                response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 64) // Can happen if spammed.
            {
                response.StatusCode = 500;
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[HomeTools] - UnBARProcess Errored out with this exception : {ex}");
                response.StatusCode = 500;
            }

            if (Directory.Exists(maindir))
                Directory.Delete(maindir, true);
        }

        public static Task ChannelIDCalculator(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                string boundary = Extensions.ExtractBoundary(request.ContentType);

                // Get the input stream from the context
                Stream inputStream = request.InputStream;

                // Create a memory stream to copy the content
                using (MemoryStream copyStream = new MemoryStream())
                {
                    // Copy the input stream to the memory stream
                    inputStream.CopyTo(copyStream);

                    // Reset the position of the copy stream to the beginning
                    copyStream.Position = 0;

                    var data = MultipartFormDataParser.Parse(copyStream, boundary);

                    int sceneid = 0;

                    string newerhome = "";

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

                    response.ContentType = "text/html";
                    response.StatusCode = (int)HttpStatusCode.OK;

                    byte[] Data = null;

                    if (newerhome == "on")
                    {
                        SceneKey sceneKey = SIDKeyGenerator.Instance.GenerateNewerType(Convert.ToUInt16(sceneid));
                        Data = Encoding.UTF8.GetBytes(PreMadeWebPages.ChannelID.Replace("PUT_GUID_HERE", sceneKey.ToString()));
                    }
                    else
                    {
                        SceneKey sceneKey = SIDKeyGenerator.Instance.Generate(Convert.ToUInt16(sceneid));
                        Data = Encoding.UTF8.GetBytes(PreMadeWebPages.ChannelID.Replace("PUT_GUID_HERE", sceneKey.ToString()));
                    }

                    response.ContentLength64 = Data.Length;

                    Stream ros = response.OutputStream;

                    if (ros.CanWrite)
                    {
                        try
                        {
                            response.ContentLength64 = Data.Length;
                            ros.Write(Data, 0, Data.Length);
                        }
                        catch (Exception)
                        {
                            // Not Important;
                        }
                    }

                    ros.Dispose();

                    copyStream.Dispose();
                }
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 64) // Can happen if spammed.
            {
                response.StatusCode = 500;
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[HomeTools] - ChannelID Calculation Errored out with this exception : {ex}");
                response.StatusCode = 500;
            }

            return Task.CompletedTask;
        }

        public static Task SceneIDCalculator(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                string boundary = Extensions.ExtractBoundary(request.ContentType);

                // Get the input stream from the context
                Stream inputStream = request.InputStream;

                // Create a memory stream to copy the content
                using (MemoryStream copyStream = new MemoryStream())
                {
                    // Copy the input stream to the memory stream
                    inputStream.CopyTo(copyStream);

                    // Reset the position of the copy stream to the beginning
                    copyStream.Position = 0;

                    var data = MultipartFormDataParser.Parse(copyStream, boundary);

                    string channelid = data.GetParameterValue("channelid");

                    string newerhome = "";

                    try
                    {
                        newerhome = data.GetParameterValue("newerhome");
                    }
                    catch (Exception)
                    {

                    }

                    if (newerhome == "on")
                    {
                        SceneKey sceneKey = new SceneKey(channelid);

                        try
                        {
                            SIDKeyGenerator.Instance.VerifyNewerKey(sceneKey);
                            ushort num = 0;

                            num = SIDKeyGenerator.Instance.ExtractSceneIDNewerType(sceneKey);

                            response.ContentType = "text/html";
                            response.StatusCode = (int)HttpStatusCode.OK;

                            byte[] Data = Encoding.UTF8.GetBytes(PreMadeWebPages.SceneID.Replace("PUT_SCENEID_HERE", num.ToString()));

                            response.ContentLength64 = Data.Length;

                            Stream ros = response.OutputStream;

                            if (ros.CanWrite)
                            {
                                try
                                {
                                    response.ContentLength64 = Data.Length;
                                    ros.Write(Data, 0, Data.Length);
                                }
                                catch (Exception)
                                {
                                    // Not Important;
                                }
                            }

                            ros.Dispose();
                        }
                        catch (SceneKeyException)
                        {
                            response.ContentType = "text/html";
                            response.StatusCode = (int)HttpStatusCode.OK;

                            byte[] Data = Encoding.UTF8.GetBytes(PreMadeWebPages.SceneID.Replace("PUT_SCENEID_HERE", "Invalid ChannelID or unsupported format"));

                            response.ContentLength64 = Data.Length;

                            Stream ros = response.OutputStream;

                            if (ros.CanWrite)
                            {
                                try
                                {
                                    response.ContentLength64 = Data.Length;
                                    ros.Write(Data, 0, Data.Length);
                                }
                                catch (Exception)
                                {
                                    // Not Important;
                                }
                            }

                            ros.Dispose();
                        }
                        catch (Exception ex)
                        {
                            ServerConfiguration.LogError($"[HomeTools] - SceneID - SceneKey Check Errored out with this exception : {ex}");
                            response.StatusCode = 500;
                        }
                    }
                    else
                    {
                        SceneKey sceneKey = new SceneKey(channelid);

                        try
                        {
                            SIDKeyGenerator.Instance.Verify(sceneKey);
                            ushort num = 0;

                            num = SIDKeyGenerator.Instance.ExtractSceneID(sceneKey);

                            response.ContentType = "text/html";
                            response.StatusCode = (int)HttpStatusCode.OK;

                            byte[] Data = Encoding.UTF8.GetBytes(PreMadeWebPages.SceneID.Replace("PUT_SCENEID_HERE", num.ToString()));

                            response.ContentLength64 = Data.Length;

                            Stream ros = response.OutputStream;

                            if (ros.CanWrite)
                            {
                                try
                                {
                                    response.ContentLength64 = Data.Length;
                                    ros.Write(Data, 0, Data.Length);
                                }
                                catch (Exception)
                                {
                                    // Not Important;
                                }
                            }

                            ros.Dispose();
                        }
                        catch (SceneKeyException)
                        {
                            response.ContentType = "text/html";
                            response.StatusCode = (int)HttpStatusCode.OK;

                            byte[] Data = Encoding.UTF8.GetBytes(PreMadeWebPages.SceneID.Replace("PUT_SCENEID_HERE", "Invalid ChannelID or unsupported format"));

                            response.ContentLength64 = Data.Length;

                            Stream ros = response.OutputStream;

                            if (ros.CanWrite)
                            {
                                try
                                {
                                    response.ContentLength64 = Data.Length;
                                    ros.Write(Data, 0, Data.Length);
                                }
                                catch (Exception)
                                {
                                    // Not Important;
                                }
                            }

                            ros.Dispose();
                        }
                        catch (Exception ex)
                        {
                            ServerConfiguration.LogError($"[HomeTools] - SceneID - SceneKey Check Errored out with this exception : {ex}");
                            response.StatusCode = 500;
                        }
                    }

                    copyStream.Dispose();
                }
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 64) // Can happen if spammed.
            {
                response.StatusCode = 500;
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[HomeTools] - SceneID Calculation Errored out with this exception : {ex}");
                response.StatusCode = 500;
            }

            return Task.CompletedTask;
        }

        public static async Task INFProcess(HttpListenerRequest request, HttpListenerResponse response)
        {
            string maindir = Directory.GetCurrentDirectory() + $"/static/HomeTools/INF_cache/{Misc.generatedynamiccacheguid(Misc.GetCurrentDateTime())}";

            try
            {
                string boundary = Extensions.ExtractBoundary(request.ContentType);

                // Get the input stream from the context
                Stream inputStream = request.InputStream;

                // Create a memory stream to copy the content
                using (MemoryStream copyStream = new MemoryStream())
                {
                    // Copy the input stream to the memory stream
                    inputStream.CopyTo(copyStream);

                    // Reset the position of the copy stream to the beginning
                    copyStream.Position = 0;

                    var data = MultipartFormDataParser.Parse(copyStream, boundary);

                    string filename = "";

                    foreach (var multipartfile in data.Files)
                    {
                        Stream filedata = multipartfile.Data;

                        filedata.Position = 0;

                        // Find the number of bytes in the stream
                        int contentLength = (int)filedata.Length;

                        // Create a byte array
                        byte[] buffer = new byte[contentLength];

                        // Read the contents of the memory stream into the byte array
                        filedata.Read(buffer, 0, contentLength);

                        filename = multipartfile.FileName;

                        string guid = Misc.generatedynamiccacheguid(filename);

                        string tempdir = $"{maindir}/{guid}";

                        if (buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0x00 && buffer[3] == 0x01 && AFSBLOWFISH.INFIVA != null)
                        {
                            Directory.CreateDirectory(tempdir);

                            buffer = AFSMISC.RemovePaddingPrefix(buffer);

                            byte[] decryptedfilebytes = AFSBLOWFISH.Crypt_Decrypt(buffer, AFSBLOWFISH.INFIVA);

                            if (decryptedfilebytes != null)
                            {
                                File.WriteAllBytes(tempdir + $"/{filename}_Processed.bin", decryptedfilebytes);

                                await FileHelper.HTTPResponseWriteFile(response, tempdir + $"/{filename}_Processed.bin");
                            }

                            if (Directory.Exists(tempdir))
                                Directory.Delete(tempdir, true);
                        }
                        else if (buffer[0] == 0xBE && buffer[1] == 0xE5 && buffer[2] == 0xBE && buffer[3] == 0xE5 && AFSBLOWFISH.INFIVA != null)
                        {
                            Directory.CreateDirectory(tempdir);

                            byte[] encryptedfilebytes = AFSBLOWFISH.Crypt_Decrypt(buffer, AFSBLOWFISH.INFIVA);

                            if (encryptedfilebytes != null)
                            {
                                File.WriteAllBytes(tempdir + $"/{filename}_Processed.bin", AFSMISC.ApplyPaddingPrefix(encryptedfilebytes));

                                await FileHelper.HTTPResponseWriteFile(response, tempdir + $"/{filename}_Processed.bin");
                            }

                            if (Directory.Exists(tempdir))
                                Directory.Delete(tempdir, true);
                        }
                    }

                    copyStream.Dispose();
                }

                response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 64) // Can happen if spammed.
            {
                response.StatusCode = 500;
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[HomeTools] - INFProcess Errored out with this exception : {ex}");
                response.StatusCode = 500;
            }

            if (Directory.Exists(maindir))
                Directory.Delete(maindir, true);
        }

        public static async Task CDSProcess(HttpListenerRequest request, HttpListenerResponse response)
        {
            string maindir = Directory.GetCurrentDirectory() + $"/static/HomeTools/CDS_cache/{Misc.generatedynamiccacheguid(Misc.GetCurrentDateTime())}";

            try
            {
                string boundary = Extensions.ExtractBoundary(request.ContentType);

                // Get the input stream from the context
                Stream inputStream = request.InputStream;

                // Create a memory stream to copy the content
                using (MemoryStream copyStream = new MemoryStream())
                {
                    // Copy the input stream to the memory stream
                    inputStream.CopyTo(copyStream);

                    // Reset the position of the copy stream to the beginning
                    copyStream.Position = 0;

                    var data = MultipartFormDataParser.Parse(copyStream, boundary);

                    string sha1 = data.GetParameterValue("sha1");

                    string filename = "";

                    foreach (var multipartfile in data.Files)
                    {
                        Stream filedata = multipartfile.Data;

                        filedata.Position = 0;

                        // Find the number of bytes in the stream
                        int contentLength = (int)filedata.Length;

                        // Create a byte array
                        byte[] buffer = new byte[contentLength];

                        // Read the contents of the memory stream into the byte array
                        filedata.Read(buffer, 0, contentLength);

                        filename = multipartfile.FileName;

                        string guid = Misc.generatedynamiccacheguid(filename);

                        string tempdir = $"{maindir}/{guid}";

                        if (sha1.Length < 16)
                            ServerConfiguration.LogWarn($"[HomeTools] - CDSProcess - Invalid SHA1 given via interface.");
                        else
                        {
                            Directory.CreateDirectory(tempdir);

                            byte[] tranformedSHA1 = BitConverter.GetBytes(AFSMISC.Sha1toNonce(AFSMISC.ConvertSha1StringToByteArray(sha1.ToUpper())));

                            if (!BitConverter.IsLittleEndian)
                                Array.Reverse(tranformedSHA1); // Reverse the byte array for big-endian

                            byte[] FileBytes = AFSBLOWFISH.Crypt_DecryptCDSContent(buffer, tranformedSHA1);

                            if (FileBytes != null)
                            {
                                /*if (filename.ToLower().EndsWith(".hcdb"))
                                    FileBytes = EDGELZMA.Decompress(FileBytes);*/

                                File.WriteAllBytes(tempdir + $"/{filename}_Processed.xml", FileBytes);

                                await FileHelper.HTTPResponseWriteFile(response, tempdir + $"/{filename}_Processed.xml");
                            }

                            if (Directory.Exists(tempdir))
                                Directory.Delete(tempdir, true);
                        }
                    }

                    copyStream.Dispose();
                }

                response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 64) // Can happen if spammed.
            {
                response.StatusCode = 500;
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[HomeTools] - CDSProcess Errored out with this exception : {ex}");
                response.StatusCode = 500;
            }

            if (Directory.Exists(maindir))
                Directory.Delete(maindir, true);
        }

        public static void UncompressFile(string compressedFilePath, string extractionFolderPath)
        {
            try
            {
                ZipFile.ExtractToDirectory(compressedFilePath, extractionFolderPath);
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[File Uncompress] - An error occurred: {ex}");
            }
        }
    }
}
