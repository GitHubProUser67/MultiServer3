using BackendProject.MiscUtils;
using CustomLogger;
using HttpMultipartParser;
using System.Text;

namespace WebUtils.PREMIUMAGENCY
{
    public class PREMIUMAGENCYClass : IDisposable
    {
        private string workpath;
        private string absolutepath;
        private string method;
        private bool disposedValue;

        public PREMIUMAGENCYClass(string method, string absolutepath, string workpath)
        {
            this.workpath = workpath;
            this.absolutepath = absolutepath;
            this.method = method;
        }

        public string? ProcessRequest(byte[]? PostData, string? ContentType)
        {
            if (string.IsNullOrEmpty(absolutepath))
                return null;

            string eventId = string.Empty;
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);
            using (MemoryStream ms = new(PostData))
            {
                var data = MultipartFormDataParser.Parse(ms, boundary);

                eventId = data.GetParameterValue("evid");

                ms.Flush();
            }

            switch (method)
            {
                case "POST":
                    switch (absolutepath)
                    {
                        case "/eventController/checkEvent.do":
                            return Event.checkEventRequestPOST(PostData, ContentType, eventId);
                        case "/eventController/entryEvent.do":
                            return Event.entryEventRequestPOST(PostData, ContentType, eventId);
                        case "/eventController/clearEvent.do":
                            return Event.clearEventRequestPOST(PostData, ContentType, eventId);
                        case "/eventController/getEventTrigger.do":
                            return Trigger.getEventTriggerRequestPOST(PostData, ContentType, workpath, eventId);
                        case "/eventController/confirmEventTrigger.do":
                            return Trigger.confirmEventTriggerRequestPOST(PostData, ContentType, workpath, eventId);
                        case "/eventController/getResource.do":
                            return Resource.getResourcePOST(PostData, ContentType, workpath);
                        case "/eventController/setUserEventCustom.do":
                            return Custom.setUserEventCustomPOST(PostData, ContentType, workpath, eventId);
                        case "/eventController/getUserEventCustom.do":
                            return Custom.getUserEventCustomRequestPOST(PostData, ContentType, workpath, eventId);
                        case "/eventController/getUserEventCustomList.do":
                            return Custom.getUserEventCustomRequestListPOST(PostData, ContentType, workpath, eventId);
                        case "/eventController/getInformationBoardSchedule.do":
                            return InfoBoard.getInformationBoardSchedulePOST(PostData, ContentType, workpath, eventId);
                        default:
                            {
                                string toTrim = "/eventController/";
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Unhandled server request discovered: {absolutepath.Replace("/eventController/", "")} | DETAILS: \n{Encoding.UTF8.GetString(PostData)}");
                            }
                            break;
                    }
                    break;
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - Unhandled Server method: {method}");
                    }
                    break;
            }

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    workpath = string.Empty;
                    absolutepath = string.Empty;
                    method = string.Empty;
                }

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~PREMIUMAGENCYClass()
        // {
        //     // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}