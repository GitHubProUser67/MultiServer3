using CustomLogger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPIService.OUWF;
using WebAPIService.UBISOFT.BuildAPI.BuildDBPullService;

namespace WebAPIService.UBISOFT.BuildAPI
{

    public class SoapBuildAPIClass : IDisposable
    {
        string workpath;
        string absolutepath;
        string method;
        private bool disposedValue;

        public SoapBuildAPIClass(string method, string absolutepath, string workpath)
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

                        case "/BuildDBPullService.asmx":
                            return BuildDBPullServiceHandler.buildDBRequestParser(PostData, ContentType);
                        default:
                            {
                                LoggerAccessor.LogError($"[BuildDBPullService] - Unhandled server request discovered: {absolutepath} | DETAILS: \n{Encoding.UTF8.GetString(PostData)}");
                            }
                            break;
                    }
                    break;
                default:
                    {
                        LoggerAccessor.LogError($"[BuildDBPullService] - Method unhandled {method}");
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
                    absolutepath = string.Empty;
                    method = string.Empty;
                    workpath = string.Empty;
                }

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~HERMESClass()
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
