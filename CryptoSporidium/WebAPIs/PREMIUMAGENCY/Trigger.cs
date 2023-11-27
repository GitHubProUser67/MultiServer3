using HttpMultipartParser;

namespace CryptoSporidium.WebAPIs.PREMIUMAGENCY
{
    public class Trigger
    {
        public static string? getEventTriggerRequestPOST(byte[]? PostData, string? ContentType, string workpath)
        {
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
                    case "76":
                        if (File.Exists($"{workpath}/eventController/MikuLiveJack/localgetEventTrigger.xml"))
                            return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/localgetEventTrigger.xml");
                        break;
                    case "63":
                        if (File.Exists($"{workpath}/eventController/MikuLiveJack/qagetEventTrigger.xml"))
                            return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/qagetEventTrigger.xml");
                        break;
                    case "90":
                        if (File.Exists($"{workpath}/eventController/MikuLiveJack/getEventTrigger.xml"))
                            return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/getEventTrigger.xml");
                        break;
                    case "91":
                        if (File.Exists($"{workpath}/eventController/MikuLiveJukebox/getEventTrigger.xml"))
                            return File.ReadAllText($"{workpath}/eventController/MikuLiveJukebox/getEventTrigger.xml");
                        break;
                    case "95":
                        if (File.Exists($"{workpath}/eventController/MikuLiveEvent/getEventTrigger.xml"))
                            return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/getEventTrigger.xml");
                        break;
                }
            }

            return null;
        }

        public static string? confirmEventTriggerRequestPOST(byte[]? PostData, string? ContentType, string workpath)
        {
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
                    case "124":
                        if (File.Exists($"{workpath}/eventController/Rainbow/localconfirmEventTrigger.xml"))
                            return File.ReadAllText($"{workpath}/eventController/Rainbow/localconfirmEventTrigger.xml");
                        break;
                    case "72":
                        if (File.Exists($"{workpath}/eventController/Rainbow/qaconfirmEventTrigger.xml"))
                            return File.ReadAllText($"{workpath}/eventController/Rainbow/qaconfirmEventTrigger.xml");
                        break;
                    case "98":
                        if (File.Exists($"{workpath}/eventController/Rainbow/confirmEventTrigger.xml"))
                            return File.ReadAllText($"{workpath}/eventController/Rainbow/confirmEventTrigger.xml");
                        break;
                    case "76":
                        if (File.Exists($"{workpath}/eventController/MikuLiveEvent/localconfirmEventTrigger.xml"))
                            return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/localconfirmEventTrigger.xml");
                        break;
                    case "55":
                        if (File.Exists($"{workpath}/eventController/MikuLiveJack/qaconfirmEventTrigger.xml"))
                            return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/qaconfirmEventTrigger.xml");
                        break;
                    case "63":
                        if (File.Exists($"{workpath}/eventController/MikuLiveEvent/qaconfirmEventTrigger.xml"))
                            return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/qaconfirmEventTrigger.xml");
                        break;
                    case "90":
                        if (File.Exists($"{workpath}/eventController/MikuLiveJack/confirmEventTrigger.xml"))
                            return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/confirmEventTrigger.xml");
                        break;
                    case "91":
                        if (File.Exists($"{workpath}/eventController/MikuLiveJukebox/confirmEventTrigger.xml"))
                            return File.ReadAllText($"{workpath}/eventController/MikuLiveJukebox/confirmEventTrigger.xml");
                        break;
                    case "95":
                        if (File.Exists($"{workpath}/eventController/MikuLiveEvent/confirmEventTrigger.xml"))
                            return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/confirmEventTrigger.xml");
                        break;
                    case "110":
                        if (File.Exists($"{workpath}/eventController/MikuLiveJack/localconfirmEventTrigger.xml"))
                            return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/localconfirmEventTrigger.xml");
                        break;
                }
            }

            return null;
        }
    }
}
