using CustomLogger;
using System;
using System.Text;

namespace WebAPIService.OUWF
{
    public class OuWFClass : IDisposable
    {
        string workpath;
        string absolutepath;
        string method;
        private bool disposedValue;

        public OuWFClass(string method, string absolutepath, string workpath)
        {
            this.workpath = workpath;
            this.absolutepath = absolutepath;
            this.method = method;
        }

        public string ProcessRequest(byte[] PostData, string ContentType)
        {
            if (string.IsNullOrEmpty(absolutepath))
                return null;

            switch (method)
            {
                case "POST":
                    switch (absolutepath)
                    {

                        case "/list/":
                            return OuWFList.List(PostData, ContentType);
                        case "/scrape/":
                            return OuWFScrape.Scrape(PostData, ContentType);
                        case "/set/":
                            return OuWFSet.Set(PostData, ContentType);
                        case "/execute/":
                            return OuWFExecute.Execute(PostData, ContentType);
                        default:
                            {
                                LoggerAccessor.LogError($"[OuWF] - Unhandled server request discovered: {absolutepath} | DETAILS: \n{Encoding.UTF8.GetString(PostData)}");
                            }
                            break;
                    }
                    break;
                default:
                    {
                        LoggerAccessor.LogError($"[OuWF] - Method unhandled {method}");
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