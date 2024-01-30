using CustomLogger;
using HttpMultipartParser;

namespace BackendProject.WebAPIs.PREMIUMAGENCY
{
    public class Trigger
    {
        public static string? getEventTriggerRequestPOST(byte[]? PostData, string? ContentType, string workpath, string eventId)
        {
            switch (eventId)
            {
                case "76":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJack/localgetEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/localgetEventTrigger.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for LOCAL MikuLiveJack {eventId}!");
                    break;
                case "63":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJack/qagetEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/qagetEventTrigger.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for QA MikuLiveJack {eventId}!");
                    break;
                case "90":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJack/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/getEventTrigger.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC MikuLiveJack {eventId}!");
                    break;
                case "91":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJukebox/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJukebox/getEventTrigger.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC MikuLiveJukebox {eventId}!");
                    break;
                case "95":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/getEventTrigger.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC MikuLiveEvent {eventId}!");
                    break;
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - GetEventTrigger unhandled for eventId {eventId} | POSTDATA: \n{PostData}");
                        return null;
                    }
            }

            return null;
        }

        public static string? confirmEventTriggerRequestPOST(byte[]? PostData, string? ContentType, string workpath, string eventId)
        {
            switch (eventId)
            {
                case "124":
                    if (File.Exists($"{workpath}/eventController/Rainbow/localconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/Rainbow/localconfirmEventTrigger.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for LOCAL Rainbow {eventId}!");
                    break;
                case "72":
                    if (File.Exists($"{workpath}/eventController/Rainbow/qaconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/Rainbow/qaconfirmEventTrigger.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for QA Rainbow {eventId}!");
                    break;
                case "98":
                    if (File.Exists($"{workpath}/eventController/Rainbow/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/Rainbow/confirmEventTrigger.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for Rainbow {eventId}!");
                    break;
                case "76":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/localconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/localconfirmEventTrigger.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for Local MikuLiveJack {eventId}!");
                    break;
                case "55":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJack/qaconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/qaconfirmEventTrigger.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for QA MikuLiveJack {eventId}!");
                    break;
                case "63":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/qaconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/qaconfirmEventTrigger.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for QA MikuLiveEvent {eventId}!");
                    break;
                case "90":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJack/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/confirmEventTrigger.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for MikuLiveJack {eventId}!");
                    break;
                case "91":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJukebox/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJukebox/confirmEventTrigger.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for MikuLiveJukebox {eventId}!");
                    break;
                case "95":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/confirmEventTrigger.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for MikuLiveEvent {eventId}!");
                    break;
                case "110":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJack/localconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/localconfirmEventTrigger.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for MikuLiveJack {eventId}!");
                    break;
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger unhandled for eventId {eventId} | POSTDATA: \n{PostData}");
                        return null;
                    }
            }

            return null;
        }
    }
}
