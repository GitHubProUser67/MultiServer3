using System.Net;
using MultiServer.CryptoSporidium.FileHelper;

namespace MultiServer.HTTPService
{
    public class FileHelper
    {
        public static Task CryptoWriteAsync(string filepath, string key, byte[] indata, bool prefercompressed)
        {
            if (filepath == null || filepath == "")
                return Task.CompletedTask;
#pragma warning disable
            Directory.CreateDirectory(Path.GetDirectoryName(filepath));
#pragma warning restore
            try
            {
                using (FileStream fs = new FileStream(filepath, FileMode.Create))
                {
                    if (key != null && key != "")
                    {
                        byte[] outfile = new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6C, 0x65, 0x64, 0x65, 0x73 };

                        byte[] ciphereddata = FileHelperCryptoClass.EncryptData(FileHelperCryptoClass.GetEncryptionKey(key), indata);

                        if (ciphereddata != null)
                        {
                            byte[] encryptedbuffer = CryptoSporidium.EDGELZMA.Compress(Misc.Combinebytearay(outfile, ciphereddata), true);

                            fs.Write(encryptedbuffer, 0, encryptedbuffer.Length);
                            fs.Flush();
                            fs.Dispose();
                        }
                    }
                    else if (prefercompressed)
                    {
                        byte[] compressedbuffer = CryptoSporidium.EDGELZMA.Compress(indata, true);

                        fs.Write(compressedbuffer, 0, compressedbuffer.Length);
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

            return Task.CompletedTask;
        }

        public static byte[] CryptoReadAsync(string filepath, string key)
        {
            if (filepath == null || filepath == "" || !File.Exists(filepath))
                return null;

            try
            {
                byte[] src = File.ReadAllBytes(filepath);

                if (src[0] == 'T' && src[1] == 'L' && src[2] == 'Z' && src[3] == 'C')
                {
                    byte[] DecompressedData = CryptoSporidium.EDGELZMA.Decompress(src);

                    if (key != null && key != "" && Misc.FindbyteSequence(DecompressedData, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 }))
                    {
                        byte[] dst = new byte[DecompressedData.Length - 9];

                        Array.Copy(DecompressedData, 9, dst, 0, dst.Length);

                        return FileHelperCryptoClass.DecryptData(dst, FileHelperCryptoClass.GetEncryptionKey(key));
                    }
                    else
                        return DecompressedData;
                }
                else
                {
                    if (key != null && key != "" && Misc.FindbyteSequence(src, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 }))
                    {
                        byte[] dst = new byte[src.Length - 9];

                        Array.Copy(src, 9, dst, 0, dst.Length);

                        return FileHelperCryptoClass.DecryptData(dst, FileHelperCryptoClass.GetEncryptionKey(key));
                    }
                    else
                        return src;
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[FileHelper - CryptoReadAsync] : Errored out with this exception : {ex}");
            }

            return null;
        }

        public static Task HTTPResponseWriteFile(HttpListenerResponse response, string path)
        {
            using (FileStream fs = File.OpenRead(path))
            {
                string filename = Path.GetFileName(path);

                response.AddHeader("Content-disposition", "attachment; filename=" + filename);

                byte[] buffer = new byte[64 * 1024];
                int read;
                using (BinaryWriter bw = new(response.OutputStream))
                {
                    while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        bw.Write(buffer, 0, read);
                        bw.Flush();
                    }

                    bw.Close();
                    bw.Dispose();
                }

                fs.Dispose();
            }

            return Task.CompletedTask;
        }
    }
}
