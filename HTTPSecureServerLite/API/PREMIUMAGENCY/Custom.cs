using HttpMultipartParser;

namespace HTTPSecureServerLite.API.PREMIUMAGENCY
{
    public class Custom
    {
        public static string? setUserEventCustomPOST(byte[]? PostData, string? ContentType)
        {
            string? output = null;
            string eventId = string.Empty;
            string? boundary = CryptoSporidium.HTTPUtils.ExtractBoundary(ContentType);

            if (boundary != null && PostData != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    eventId = data.GetParameterValue("evid");

                    ms.Flush();
                }

                switch (eventId)
                {
                    case "95":
                        if (File.Exists($"{HTTPSServerConfiguration.HTTPSStaticFolder}/eventController/MikuLiveEvent/setUserEventCustom.xml"))
                            output = File.ReadAllText($"{HTTPSServerConfiguration.HTTPSStaticFolder}/eventController/MikuLiveEvent/setUserEventCustom.xml");
                        break;
                }
            }

            return output;
        }
    }
}
