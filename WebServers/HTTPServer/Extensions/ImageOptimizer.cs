using CustomLogger;
using System;
using System.Diagnostics;
using System.IO;

namespace HTTPServer.Extensions
{
    public class ImageOptimizer
    {
        private static string maindir = HTTPServerConfiguration.APIStaticFolder + "/ImageUpscale/Cache";

        public static byte[] OptimizeImage(string imagePath, int num)
        {
            string tempOutfilePath = maindir + $"/{num:X4}.tmp";
            string finalOutfilePath = maindir + $"/{num:X4}{Path.GetExtension(imagePath)}";

            try
            {
                // If the final output file exists and is recent enough, return it
                if (File.Exists(finalOutfilePath) && File.GetCreationTime(finalOutfilePath) > DateTime.Now.AddHours(-4))
                    return File.ReadAllBytes(finalOutfilePath);

                string? convertFileName = null;

                switch (GetCPUArchitecture())
                {
                    case "x86_32":
                        convertFileName = $"{Directory.GetCurrentDirectory()}/static/converters/ImageMagick/convert32";
                        break;
                    case "AMD64":
                        convertFileName = $"{Directory.GetCurrentDirectory()}/static/converters/ImageMagick/convert64";
                        break;
                    case "ARM_64":
                        convertFileName = $"{Directory.GetCurrentDirectory()}/static/converters/ImageMagick/convertARM";
                        break;
                }

                if (!string.IsNullOrEmpty(convertFileName))
                {
                    Directory.CreateDirectory(maindir);

                    try
                    {
                        using Process? process = Process.Start(new ProcessStartInfo()
                        {
                            FileName = convertFileName,
                            Arguments = $"\"{imagePath}\" -strip -antialias -interlace Plane -quality 75 \"{tempOutfilePath}\"",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            WorkingDirectory = $"{Directory.GetCurrentDirectory()}/static/converters/ImageMagick/",
                            CreateNoWindow = true
                        });

                        process?.WaitForExit();

                        int? exitCode = process?.ExitCode;

                        if (exitCode.HasValue && exitCode.Value == 0)
                        {
                            // Only rename to the final path if the process was successful
                            File.Move(tempOutfilePath, finalOutfilePath, true);
                            return File.ReadAllBytes(finalOutfilePath);
                        }
                        else
                            LoggerAccessor.LogWarn($"[HTTP] - ImageMagick conversion process failed with status code: {exitCode}");
                    }
                    catch (Exception ex)
                    {
                        LoggerAccessor.LogWarn($"[HTTP] - ImageMagick conversion process failed - {ex}");
                    }
                    finally
                    {
                        // Ensure temporary file is deleted if exists
                        if (File.Exists(tempOutfilePath))
                            File.Delete(tempOutfilePath);
                    }
                }
                else
                    LoggerAccessor.LogWarn("[HTTP] - Could not find ImageMagick for current architecture, aborting convert process.");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[HTTP] - ImageUpscaler failed - {ex}");
            }

            return File.ReadAllBytes(imagePath);
        }

        private static string GetCPUArchitecture()
        {
            string? processorArchitecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");

            if (!string.IsNullOrEmpty(processorArchitecture))
                return processorArchitecture + ((processorArchitecture == "AMD64") ? string.Empty : (Environment.Is64BitProcess ? "_64" : "_32"));

            // Unsupported architecture or unable to determine
            return "Unknown";
        }
    }
}
