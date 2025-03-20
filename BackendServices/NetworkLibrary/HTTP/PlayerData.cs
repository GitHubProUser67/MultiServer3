using NetworkLibrary.Extension;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace NetworkLibrary.HTTP
{
    internal static class PlayerData
    {
        public static double? GetVBitRate(string jsonInfoData)
        {
            JObject jsonObject = JObject.Parse(jsonInfoData);

            var videoStreams = jsonObject["media_info"]?["streams"]?
                .Where(s => (string)s?["codec_type"] == "video");

            if (videoStreams != null)
            {
                foreach (var stream in videoStreams)
                {
                    string bitRateStr = (string)stream?["bit_rate"];
                    if (!string.IsNullOrEmpty(bitRateStr) && double.TryParse(bitRateStr, out double bitRate))
                        return bitRate / 1024; // Convert bps to kbps
                }
            }

            return null;
        }

        public static double? GetVFrameRate(string jsonInfoData)
        {
            JObject jsonObject = JObject.Parse(jsonInfoData);

            var videoStreams = jsonObject["media_info"]?["streams"]?
                .Where(s => (string)s?["codec_type"] == "video");

            if (videoStreams != null)
            {
                foreach (var stream in videoStreams)
                {
                    string rFrameRate = (string)stream?["r_frame_rate"];
                    if (!string.IsNullOrEmpty(rFrameRate))
                        return rFrameRate.Eval();
                }
            }

            return null;
        }
    }
}
