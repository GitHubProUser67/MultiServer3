using CustomLogger;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NetworkLibrary.Upscalers;

public static class ImageOptimizer
{
    private static string tmpDir = $"{Path.GetTempPath()}/ImageUpscale/Cache";

    public static Stream OptimizeImage(string convertersDir, string imagePath, string extension,
        string CommandLineParametersConvert = "-colorspace HSL -enhance -channel B -evaluate multiply 1.00 -channel G -evaluate multiply 1.15 +channel -colorspace sRGB -filter Lanczos -quality 92 -antialias",
        string CommandLineParametersFsr = "-QualityMode UltraQuality -Scale 2x 2x",
        bool upscale = true)
    {
        string tempfilePath = Path.Combine(tmpDir, $"{Guid.NewGuid()}_tmp{extension}");
        string tempScaledfilePath = Path.Combine(tmpDir, $"{Guid.NewGuid()}_Scaled{extension}");
        string tempSharpenedfilePath = Path.Combine(tmpDir, $"{Guid.NewGuid()}_Sharpened{extension}");
        string convertFilePath = null;
        switch (RuntimeInformation.OSArchitecture)
        {
            case Architecture.X86:
                convertFilePath = "32";
                break;
            case Architecture.X64:
                convertFilePath = "64";
                break;
            case Architecture.Arm64:
                convertFilePath = "ARM";
                break;
        }
        if (!string.IsNullOrEmpty(convertFilePath) && Directory.GetFiles($"{convertersDir}/ImageMagick/", $"convert{convertFilePath}*").Length > 0)
        {
            convertFilePath = $"{convertersDir}/ImageMagick/convert{convertFilePath}";
            try
            {
                Directory.CreateDirectory(tmpDir);
                using (Process magickProc = Process.Start(new ProcessStartInfo
                {
                    FileName = convertFilePath,
                    Arguments = $"\"{imagePath}\" {CommandLineParametersConvert} \"{tempfilePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }))
                {
                    magickProc?.WaitForExit();
                    if (magickProc?.ExitCode is 0)
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
                                    // FidelityFx doesn't work well with transparency data...
                                    if (HasTransparentPixels(tempfilePath))
                                        return new MemoryStream(File.ReadAllBytes(tempfilePath));
                                    convertFilePath = null;
                                    switch (RuntimeInformation.OSArchitecture)
                                    {
                                        case Architecture.X86:
                                        case Architecture.X64:
                                            convertFilePath = $"{convertersDir}/FidelityFx/FidelityFX_CLI.exe";
                                            break;
                                    }
                                    if (!string.IsNullOrEmpty(convertFilePath) && File.Exists(convertFilePath))
                                    {
                                        if (upscale)
                                        {
                                            try
                                            {
                                                using (Process upscaleProc = Process.Start(new ProcessStartInfo
                                                {
                                                    FileName = convertFilePath,
                                                    Arguments = $"-Mode EASU {CommandLineParametersFsr} \"{tempfilePath}\" \"{tempScaledfilePath}\"",
                                                    RedirectStandardOutput = true,
                                                    RedirectStandardError = true,
                                                    UseShellExecute = false,
                                                    CreateNoWindow = true
                                                }))
                                                {
                                                    upscaleProc?.WaitForExit();
                                                    if (upscaleProc?.ExitCode is 0)
                                                    {
                                                        try
                                                        {
                                                            using (Process sharpenProc = Process.Start(new ProcessStartInfo
                                                            {
                                                                FileName = convertFilePath,
                                                                Arguments = $"-Mode RCAS \"{tempScaledfilePath}\" \"{tempSharpenedfilePath}\"",
                                                                RedirectStandardOutput = true,
                                                                RedirectStandardError = true,
                                                                UseShellExecute = false,
                                                                CreateNoWindow = true
                                                            }))
                                                            {
                                                                sharpenProc?.WaitForExit();
                                                                if (sharpenProc?.ExitCode is 0)
                                                                    return new MemoryStream(File.ReadAllBytes(tempSharpenedfilePath));
                                                                else
                                                                    LoggerAccessor.LogWarn($"[ImageOptimizer] - FidelityFX_CLI sharpen process failed with status code: {sharpenProc?.ExitCode}");
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            LoggerAccessor.LogWarn($"[ImageOptimizer] - FidelityFX_CLI sharpen process failed - {ex}");
                                                        }
                                                    }
                                                    else
                                                        LoggerAccessor.LogWarn($"[ImageOptimizer] - FidelityFX_CLI upscale process failed with status code: {upscaleProc?.ExitCode}");
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
                                                using (Process sharpenProc = Process.Start(new ProcessStartInfo
                                                {
                                                    FileName = convertFilePath,
                                                    Arguments = $"-Mode RCAS \"{tempfilePath}\" \"{tempSharpenedfilePath}\"",
                                                    RedirectStandardOutput = true,
                                                    RedirectStandardError = true,
                                                    UseShellExecute = false,
                                                    CreateNoWindow = true
                                                }))
                                                {
                                                    sharpenProc?.WaitForExit();
                                                    if (sharpenProc?.ExitCode is 0)
                                                        return new MemoryStream(File.ReadAllBytes(tempSharpenedfilePath));
                                                    else
                                                        LoggerAccessor.LogWarn($"[ImageOptimizer] - FidelityFX_CLI sharpen process failed with status code: {sharpenProc?.ExitCode}");
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
                        return new MemoryStream(File.ReadAllBytes(tempfilePath));
                    }
                    else
                        LoggerAccessor.LogWarn($"[ImageOptimizer] - ImageMagick conversion process failed with status code: {magickProc?.ExitCode}");
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogWarn($"[ImageOptimizer] - ImageMagick conversion process failed - {ex}");
            }
            finally
            {
                _ = Task.Run(() =>
                {
                    if (File.Exists(tempfilePath)) File.Delete(tempfilePath);
                    if (File.Exists(tempScaledfilePath)) File.Delete(tempScaledfilePath);
                    if (File.Exists(tempSharpenedfilePath)) File.Delete(tempSharpenedfilePath);
                });
            }
        }
        else
            LoggerAccessor.LogWarn("[ImageOptimizer] - Could not find ImageMagick for current architecture, aborting convert process.");
        return File.OpenRead(imagePath);
    }

    private static bool HasTransparentPixels(string imagePath)
    {
        bool HasTransparency = false;

        try
        {
            using (Image<Rgba32> image = Image.Load<Rgba32>(imagePath))
                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        Span<Rgba32> pixelRow = accessor.GetRowSpan(y);

                        // pixelRow.Length has the same value as accessor.Width,
                        // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                        for (int x = 0; x < pixelRow.Length; x++)
                        {
                            // Get a reference to the pixel at position x
                            ref Rgba32 pixel = ref pixelRow[x];
                            if (pixel.A < byte.MaxValue)
                            {
                                HasTransparency = true;
                                return;
                            }
                        }
                    }
                });
        }
        catch
        {
            throw;
        }

        return HasTransparency;
    }
}