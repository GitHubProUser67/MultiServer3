using System.Net;
using System.Text;

namespace PSMultiServer.PoodleHTTP
{
    public static class FileHelper
    {
        public static async Task CryptoWriteAsync(string filepath, string key, byte[] indata)
        {
            if (filepath == null)
                return;
            Directory.CreateDirectory(Path.GetDirectoryName(filepath));

            try
            {
                using (FileStream fs = new FileStream(filepath, FileMode.Create))
                {
                    if (key != null && key != "")
                    {
                        byte[] outfile = new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6C, 0x65, 0x64, 0x65, 0x73 };

                        byte[] encryptedbuffer = CryptoSporidium.EDGELZMA.Compress(Misc.Combinebytearay(outfile, CryptoSporidium.TRIPLEDES.EncryptData(CryptoSporidium.TRIPLEDES.GetEncryptionKey(key), indata)), true);

                        fs.Write(encryptedbuffer, 0, encryptedbuffer.Length);
                        fs.Flush();
                        fs.Dispose();
                    }
                    else
                    {
                        fs.Write(indata, 0, indata.Length);
                        fs.Flush();
                        fs.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[FileHelper - CryptoWriteAsyncFullPath] : Errored out with this exception : {ex}");
            }
        }

        public static async Task<byte[]> CryptoReadAsync(string filepath, string key)
        {
            if (filepath == null || !File.Exists(filepath))
                return Encoding.UTF8.GetBytes("");

            try
            {
                byte[] firstFourBytes = new byte[4];

                using (FileStream fileStream = new(filepath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.Read(firstFourBytes, 0, 4);
                    fileStream.Close();
                }

                if (firstFourBytes[0] == 'T' && firstFourBytes[1] == 'L' && firstFourBytes[2] == 'Z' && firstFourBytes[3] == 'C')
                {
                    byte[] DecompressedData = CryptoSporidium.EDGELZMA.Decompress(File.ReadAllBytes(filepath));
                    byte[] CryptoTest = new byte[9];
                    Array.Copy(DecompressedData, 0, CryptoTest, 0, CryptoTest.Length);

                    if (key != null && key != "" && Misc.FindbyteSequence(CryptoTest, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 }))
                    {
                        byte[] dst = new byte[DecompressedData.Length - 9];

                        Array.Copy(DecompressedData, 9, dst, 0, dst.Length);

                        return CryptoSporidium.TRIPLEDES.DecryptData(dst,
                                    CryptoSporidium.TRIPLEDES.GetEncryptionKey(key));
                    }
                    else
                    {
                        return DecompressedData;
                    }
                }
                else
                {
                    byte[] firstNineBytes = new byte[9];

                    using (FileStream fileStream = new(filepath, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.Read(firstNineBytes, 0, 9);
                        fileStream.Close();
                    }

                    if (key != null && key != "" && Misc.FindbyteSequence(firstNineBytes, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 }))
                    {
                        byte[] src = File.ReadAllBytes(filepath);
                        byte[] dst = new byte[src.Length - 9];

                        Array.Copy(src, 9, dst, 0, dst.Length);

                        return CryptoSporidium.TRIPLEDES.DecryptData(dst,
                                    CryptoSporidium.TRIPLEDES.GetEncryptionKey(key));
                    }
                    else
                    {
                        return File.ReadAllBytes(filepath);
                    }
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[FileHelper - CryptoReadAsync] : Errored out with this exception : {ex}");

                return Encoding.UTF8.GetBytes("");
            }
        }

        public static async Task WriteAsync(string path, HttpListenerRequest request)
        {
            // Get the input stream from the context
            Stream inputStream = request.InputStream;

            using (MemoryStream ms = new MemoryStream())
            {
                inputStream.CopyTo(ms);

                // Reset the memory stream position to the beginning
                ms.Position = 0;

                // Find the number of bytes in the stream
                int contentLength = (int)ms.Length;

                // Create a byte array
                byte[] buffer = new byte[contentLength];

                // Read the contents of the memory stream into the byte array
                ms.Read(buffer, 0, contentLength);

                await CryptoWriteAsync(path, HTTPPrivateKey.HTTPPrivatekey, buffer);

                ms.Dispose();
            }
        }

        public static async Task ReadAsync(string path, Func<Stream, Task> reader)
        {
            try
            {
                await CriticalReadAsync(path, reader);
            }
            catch (Exception ex)
            {
                string backupPath = BackupPath(path);
                if (!File.Exists(backupPath))
                {
                    throw;
                }

                ServerConfiguration.LogWarn($"Can not read {path}, turn back to backup.", ex);
                await CriticalReadAsync(backupPath, reader);
            }
        }

        private static async Task CriticalReadAsync(string path, Func<Stream, Task> reader)
        {
            // Convert byte array to MemoryStream
            using (Stream stream = new MemoryStream(await CryptoReadAsync(path, HTTPPrivateKey.HTTPPrivatekey)))
            {
                await reader(stream);
            }
        }

        private static string BackupPath(string path) => $"{path}.backup";
    }
}
