using BackendProject.MiscUtils;
using CustomLogger;
using System.Diagnostics;

namespace HTTPServer.Extensions
{
    public static class ImageUpscaler
    {
        public static Task<byte[]>? UpscaleImage(string imagePath, string crc32)
        {
            string maindir = HTTPServerConfiguration.APIStaticFolder + "/ImageUpscale/Cache";

            string tmpfilepath = maindir + $"/{crc32}.tmp";

            int num = 0;
            foreach (char value in crc32 + "0utput".ToLower().Replace(Path.DirectorySeparatorChar, '/')) // 0utput is a anti-collision salt, cache can get big.
            {
                num *= 37;
                num += Convert.ToInt32(value);
            }

            string outfilepath = maindir + $"/{num:X8}.tmp";

            if (File.Exists(tmpfilepath))
                File.Delete(tmpfilepath);

            if (File.Exists(outfilepath))
            {
                if (File.GetCreationTime(outfilepath) <= DateTime.Now.AddDays(7)) // 7 days of cache period.
                    File.Delete(outfilepath);
                else
                    return Task.FromResult(File.ReadAllBytes(outfilepath));
            }

            byte[] indata = File.ReadAllBytes(imagePath);

            if (indata != null)
            {
                Directory.CreateDirectory(maindir);
                File.WriteAllBytes(tmpfilepath, indata);
            }
            else
                return null;

            try
            {
                string? ConvertfileName = null;
                
                switch (VariousUtils.GetCPUArchitecture())
                {
                    case "x86_32":
                        ConvertfileName = $"{Directory.GetCurrentDirectory()}/static/converters/ImageMagick/convert32";
                        break;
                    case "AMD64":
                        ConvertfileName = $"{Directory.GetCurrentDirectory()}/static/converters/ImageMagick/convert64";
                        break;
                    case "ARM_64":
                        ConvertfileName = $"{Directory.GetCurrentDirectory()}/static/converters/ImageMagick/convertARM";
                        break;
                }

                if (!string.IsNullOrEmpty(ConvertfileName))
                {
                    using Process? process = Process.Start(new ProcessStartInfo()
                    {
                        FileName = ConvertfileName,
                        Arguments = $"\"{tmpfilepath}\" -antialias \"{outfilepath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        WorkingDirectory = $"{Directory.GetCurrentDirectory()}/static/converters/ImageMagick/", // Can load various config files.
                        CreateNoWindow = false // This is a console app.
                    });

                    process?.WaitForExit();

                    if (process?.ExitCode == 0)
                    {
                        if (File.Exists(tmpfilepath))
                            File.Delete(tmpfilepath);
                        return Task.FromResult(File.ReadAllBytes(outfilepath));
                    }
                    else
                        LoggerAccessor.LogWarn("[HTTP] - ImageMagick conversion process failed.");
                }
                else
                    LoggerAccessor.LogWarn("[HTTP] - Could not find ImageMagick for current architecture, aborting convert process.");

                if (File.Exists(tmpfilepath))
                    File.Delete(tmpfilepath);
                return Task.FromResult(indata);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[HTTP] - ImageMagick conversion process failed - {ex}");
            }

            return null;
        }
    }
}
