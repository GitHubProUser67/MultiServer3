using CustomLogger;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkLibrary.Upscalers
{
    public static class ImageOptimizer
    {
        private static string tmpDir = Path.GetTempPath() + "/ImageUpscale/Cache";

        public static Stream OptimizeImage(string convertersDir, string imagePath, string extension,
            string CommandLineParametersConvert = "-colorspace HSL -enhance -channel B -evaluate multiply 1.00 -channel G -evaluate multiply 1.15 +channel -colorspace sRGB -filter Lanczos -quality 92 -antialias",
            string CommandLineParametersFsr = "-QualityMode UltraQuality -Scale 2x 2x",
            bool upscale = true)
        {
            string convertFilePath = null;
            string tempfilePath = Path.Combine(tmpDir, Guid.NewGuid().ToString() + "_tmp" + extension);
            string tempScaledfilePath = Path.Combine(tmpDir, Guid.NewGuid().ToString() + "_Scaled" + extension);
            string tempSharpenedfilePath = Path.Combine(tmpDir, Guid.NewGuid().ToString() + "_Sharpened" + extension);

            switch (GetCPUArchitecture())
            {
                case "x86_32":
                    convertFilePath = "32";
                    break;
                case "AMD64":
                    convertFilePath = "64";
                    break;
                case "ARM_64":
                    convertFilePath = "ARM";
                    break;
            }

            if (!string.IsNullOrEmpty(convertFilePath) && Directory.GetFiles($"{convertersDir}/ImageMagick/", $"convert{convertFilePath}*").Length > 0)
            {
                convertFilePath = $"{convertersDir}/ImageMagick/convert{convertFilePath}";

                try
                {
                    Directory.CreateDirectory(tmpDir);

                    using (Process process = Process.Start(new ProcessStartInfo()
                    {
                        FileName = convertFilePath,
                        Arguments = $"\"{imagePath}\" {CommandLineParametersConvert} \"{tempfilePath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }))
                    {
                        process?.WaitForExit();

                        int? exitCode = process?.ExitCode;

                        if (exitCode.HasValue && exitCode.Value == 0)
                        {
                            if (Extension.Windows.Win32API.IsWindows)
                            {
                                switch (extension.Substring(1).ToLower())
                                {
                                    case "bmp":
                                    case "png":
                                    case "ico":
                                    case "jpg":
                                    case "tif":
                                    case "gif":
                                        convertFilePath = null;

                                        switch (GetCPUArchitecture())
                                        {
                                            case "x86_32":
                                            case "AMD64":
                                                convertFilePath = $"{convertersDir}/FidelityFx/FidelityFX_CLI.exe";
                                                break;
                                        }

                                        if (!string.IsNullOrEmpty(convertFilePath) && File.Exists(convertFilePath))
                                        {
                                            if (upscale)
                                            {
                                                try
                                                {
                                                    using (Process UpscaleProc = Process.Start(new ProcessStartInfo()
                                                    {
                                                        FileName = convertFilePath,
                                                        Arguments = $"-Mode EASU {CommandLineParametersFsr} \"{tempfilePath}\" \"{tempScaledfilePath}\"",
                                                        RedirectStandardOutput = true,
                                                        RedirectStandardError = true,
                                                        UseShellExecute = false,
                                                        CreateNoWindow = true
                                                    }))
                                                    {
                                                        UpscaleProc?.WaitForExit();

                                                        exitCode = UpscaleProc?.ExitCode;

                                                        if (exitCode.HasValue && exitCode.Value == 0)
                                                        {
                                                            try
                                                            {
                                                                using (Process SharpenProc = Process.Start(new ProcessStartInfo()
                                                                {
                                                                    FileName = convertFilePath,
                                                                    Arguments = $"-Mode RCAS \"{tempScaledfilePath}\" \"{tempSharpenedfilePath}\"",
                                                                    RedirectStandardOutput = true,
                                                                    RedirectStandardError = true,
                                                                    UseShellExecute = false,
                                                                    CreateNoWindow = true
                                                                }))
                                                                {
                                                                    SharpenProc?.WaitForExit();

                                                                    exitCode = SharpenProc?.ExitCode;

                                                                    if (exitCode.HasValue && exitCode.Value == 0)
                                                                        return File.OpenRead(tempSharpenedfilePath);
                                                                    else
                                                                        LoggerAccessor.LogWarn($"[ImageOptimizer] - FidelityFX_CLI sharpen process failed with status code: {exitCode}");
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                LoggerAccessor.LogWarn($"[ImageOptimizer] - FidelityFX_CLI sharpen process failed - {ex}");
                                                            }
                                                        }
                                                        else
                                                            LoggerAccessor.LogWarn($"[ImageOptimizer] - FidelityFX_CLI upscale process failed with status code: {exitCode}");
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    LoggerAccessor.LogWarn($"[ImageOptimizer] - FidelityFX_CLI upscale process failed - {ex}");
                                                }
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    using (Process SharpenProc = Process.Start(new ProcessStartInfo()
                                                    {
                                                        FileName = convertFilePath,
                                                        Arguments = $"-Mode RCAS \"{tempfilePath}\" \"{tempSharpenedfilePath}\"",
                                                        RedirectStandardOutput = true,
                                                        RedirectStandardError = true,
                                                        UseShellExecute = false,
                                                        CreateNoWindow = true
                                                    }))
                                                    {
                                                        SharpenProc?.WaitForExit();

                                                        exitCode = SharpenProc?.ExitCode;

                                                        if (exitCode.HasValue && exitCode.Value == 0)
                                                            return File.OpenRead(tempSharpenedfilePath);
                                                        else
                                                            LoggerAccessor.LogWarn($"[ImageOptimizer] - FidelityFX_CLI sharpen process failed with status code: {exitCode}");
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    LoggerAccessor.LogWarn($"[ImageOptimizer] - FidelityFX_CLI sharpen process failed - {ex}");
                                                }
                                            }
                                        }
                                        else
                                            LoggerAccessor.LogWarn("[ImageOptimizer] - Could not find FidelityFX_CLI for current architecture, aborting process.");
                                        break;
                                    default:
#if DEBUG
                                        LoggerAccessor.LogWarn("[ImageOptimizer] - Input file is not compatible with FidelityFX_CLI, skipping process.");
#endif
                                        break;
                                }
                            }

                            return File.OpenRead(tempfilePath);
                        }
                        else
                            LoggerAccessor.LogWarn($"[ImageOptimizer] - ImageMagick conversion process failed with status code: {exitCode}");
                    }
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogWarn($"[ImageOptimizer] - ImageMagick conversion process failed - {ex}");
                }
                finally
                {
                    new Task(() =>
                    {
                        //wait 1 minute for let client time to download the file
                        Thread.Sleep(60000);
#if DEBUG
                        LoggerAccessor.LogWarn($"[ImageOptimizer] - Removing temporary file.");
#endif
                        if (File.Exists(tempfilePath)) File.Delete(tempfilePath);
                        if (File.Exists(tempScaledfilePath)) File.Delete(tempScaledfilePath);
                        if (File.Exists(tempSharpenedfilePath)) File.Delete(tempSharpenedfilePath);
                    }).Start();
                }
            }
            else
                LoggerAccessor.LogWarn("[ImageOptimizer] - Could not find ImageMagick for current architecture, aborting convert process.");

            return File.OpenRead(imagePath);
        }

        private static string GetCPUArchitecture()
        {
            string processorArchitecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");

            if (!string.IsNullOrEmpty(processorArchitecture))
                return processorArchitecture + ((processorArchitecture == "AMD64") ? string.Empty : (Environment.Is64BitProcess ? "_64" : "_32"));

            // Unsupported architecture or unable to determine
            return "Unknown";
        }
    }
}
