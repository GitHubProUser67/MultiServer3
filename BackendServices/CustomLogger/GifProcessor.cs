using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using Spectre.Console;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CustomLogger
{
    public class GifProcessor
    {
        public static async Task PrintGifToConsole(string GiffilePath)
        {
            await AnsiConsole.Live(Text.Empty)
            .StartAsync(async ctx =>
            {
                using Image gif = await Image<Rgba32>.LoadAsync(GiffilePath);
                GifFrameMetadata metadata = gif.Frames.RootFrame.Metadata.GetGifMetadata();

                for (int i = 0; i < gif.Frames.Count; i++)
                {
                    using Image clone = gif.Frames.CloneFrame(i);

                    await using MemoryStream memoryStream = new MemoryStream();
                    await clone.SaveAsBmpAsync(memoryStream);
                    memoryStream.Position = 0;

                    ctx.UpdateTarget(new CanvasImage(memoryStream));

                    // FrameDelay is measured in 1/100th second.
                    // Let's half the delay since we're encoding per frame.
                    await Task.Delay(TimeSpan.FromMilliseconds(gif.Frames[i].Metadata.GetGifMetadata().FrameDelay * 5));
                }
            });
        }
    }
}
