using NetworkLibrary.HTTP;
using System;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace WebAPIService.JsWebMedia
{
    // From: https://www.codeproject.com/Articles/1079119/Video-Transcoding-and-Streaming-on-the-fly
    public class MediaInfo
    {
        private string workpath;
        private string convertersPath;
        private string fullurl;
        private string filepath;

        public MediaInfo(string workpath, string convertersPath, string fullurl, string filepath)
        {
            this.workpath = workpath;
            this.convertersPath = convertersPath;
            this.fullurl = fullurl;
            this.filepath = filepath;
        }

        public bool IsSupported()
        {
            if (File.Exists(string.Format(@"{0}/{1}", workpath,
                HttpUtility.UrlDecode(MediaBrowse.GetQueryParameter(HTTPProcessor.ExtractQueryString(fullurl), "item")))
                .Replace(@"\\", @"\").Replace("//", "/")))
                return true;

            return false;
        }

        public (ushort, string, string) StartFFProbe()
        {
            try
            {
                string path = string.Format(@"{0}/{1}", workpath,
                    HttpUtility.UrlDecode(MediaBrowse.GetQueryParameter(HTTPProcessor.ExtractQueryString(fullurl), "item")))
                    .Replace(@"\\", @"\").Replace("//", "/");

                ProcessStartInfo pr = new ProcessStartInfo(convertersPath + "/ffprobe",
                    string.Format(@"-i ""{0}"" -show_streams -show_format -print_format json", path));
                pr.UseShellExecute = false;
                pr.RedirectStandardOutput = true;
                pr.CreateNoWindow = true;

                Process proc = new Process();
                proc.StartInfo = pr;
                proc.Start();

                string output = "{ \"seeking_type\": \"" + GetSeekingType(new FileInfo(path).Extension) + "\", \"media_info\": " + proc.StandardOutput.ReadToEnd() + " }";

                proc.WaitForExit();

                return (200, "text/plain", output);
            }
            catch (Exception e)
            {
                CustomLogger.LoggerAccessor.LogError(string.Format("[MediaInfo] - Media.StartFFProbe, Exception : {0}", e.Message));
            }

            return (500, "text/plain", string.Empty);
        }

        public static string GetSeekingType(string fileExt)
        {
            switch (fileExt)
            {
                case ".mp4":
                    return "client";
                default:
                    return "server";
            }
        }
    }
}
