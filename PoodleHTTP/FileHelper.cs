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

                        byte[] encryptedbuffer = Misc.Combinebytearay(outfile, CryptoSporidium.TRIPLEDES.EncryptData(CryptoSporidium.TRIPLEDES.GetEncryptionKey(key), indata));

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
            if (filepath == null || !Directory.Exists(Path.GetDirectoryName(filepath)))
                return Encoding.UTF8.GetBytes("ERROR IN - FileHelper - CryptoReadAsync");

            try
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
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[FileHelper - CryptoReadAsync] : Errored out with this exception : {ex}");

                return Encoding.UTF8.GetBytes("ERROR IN - FileHelper - CryptoReadAsync");
            }
        }

        public static async Task WriteAsync(string path, Func<Stream, Task> writer, bool isBackup = true)
        {
            string tempFilePath = $"{path}.writing";
            using (var stream = new FileStream(
                tempFilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                0x10000,
                FileOptions.SequentialScan)
            )
            {
                await writer(stream);
            }

            if (File.Exists(path))
            {
                await SpinRetry(() => File.Replace(tempFilePath, path, isBackup ? BackupPath(path) : null, true));
            }
            else
            {
                await SpinRetry(() => File.Move(tempFilePath, path));
            }
        }

        public static async Task ReadAsync(string path, Func<Stream, Task> reader)
        {
            try
            {
                await CriticalReadAsync(path, reader);
            }
            catch (Exception e)
            {
                string backupPath = BackupPath(path);
                if (!File.Exists(backupPath))
                {
                    throw;
                }

                ServerConfiguration.LogWarn($"Can not read {path}, turn back to backup.", e);
                await CriticalReadAsync(backupPath, reader);
            }
        }

        private static async Task CriticalReadAsync(string path, Func<Stream, Task> reader)
        {
            using FileStream stream = new(
                path,
                FileMode.OpenOrCreate,
                FileAccess.Read,
                FileShare.ReadWrite,
                0x10000,
                FileOptions.SequentialScan);
            await reader(stream);
        }

        private static string BackupPath(string path) => $"{path}.backup";

        private static async Task SpinRetry(Action action, int retryCount = 10)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    action();
                    await Task.Delay(100);
                    break;
                }
                catch
                {
                    if (i == retryCount - 1)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
