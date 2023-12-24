using HttpMultipartParser;

namespace BackendProject.WebAPIs.PREMIUMAGENCY
{
    public class Custom
    {
        public static string? setUserEventCustomPOST(byte[]? PostData, string? ContentType, string workpath)
        {
            string? output = null;
            string eventId = string.Empty;
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);

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
                        if (File.Exists($"{workpath}/eventController/MikuLiveEvent/setUserEventCustom.xml"))
                            output = File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/setUserEventCustom.xml");
                        break;
                }
            }

            return output;
        }
    }
}
