using CustomLogger;
using WebUtils.NDREAMS.Aurora;
using WebUtils.NDREAMS.Fubar;

namespace WebUtils.NDREAMS
{
    public class NDREAMSClass : IDisposable
    {
        private bool disposedValue;
        private string absolutepath;
        private string fullurl;
        private string apipath;
        private string method;

        public NDREAMSClass(string method, string fullurl, string absolutepath, string apipath)
        {
            this.absolutepath = absolutepath;
            this.fullurl = fullurl;
            this.method = method;
            this.apipath = apipath;
        }

        public string? ProcessRequest(Dictionary<string, string>? QueryParameters, byte[]? PostData = null, string? ContentType = null)
        {
            if (string.IsNullOrEmpty(absolutepath))
                return null;

            switch (method)
            {
                case "POST":
                    switch (absolutepath)
                    {
                        case "/fubar/fisi.php":
                            return fisi.fisiProcess(PostData, ContentType);
                        case "/aurora/visit.php":
                            return visitClass.ProcessVisit(PostData, ContentType, apipath);
                        case "/aurora/MysteryItems/mystery3.php":
                            return Mystery3.ProcessMystery3(PostData, ContentType, fullurl, apipath);
                        case "/Teaser/beans.php":
                            return Teaser.ProcessBeans(PostData, ContentType);
                        default:
                            LoggerAccessor.LogWarn($"[NDREAMS] - Unknown method: {absolutepath} was requested. Please report to GITHUB");
                            break;
                    }
                    break;
                default:
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
                    absolutepath = string.Empty;
                    method = string.Empty;
                }

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~NDREAMSClass()
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
