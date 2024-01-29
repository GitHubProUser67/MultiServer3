using CustomLogger;

namespace BackendProject.WebAPIs.PREMIUMAGENCY
{
    public class Custom
    {
        public static string? setUserEventCustomPOST(byte[]? PostData, string? ContentType, string workpath, string eventId)
        {
            string? output = null;
            switch (eventId)
            {
                case "95":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/setUserEventCustom.xml"))
                        output = File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/setUserEventCustom.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - SetUserEventCustom sent for MikuLiveEvent {eventId}!");


                    break;
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - SetUserEventCustom unhandled for eventId {eventId}");
                        return null;
                    }
            }

            return output;
        }
    }
}