
using System;
using System.IO;

namespace WebAPIService.CAPONE
{
    public class CAPONEClass : IDisposable
    {
        private string workPath;
        private string absolutePath;
        private string method;
        private bool disposedValue;

        public CAPONEClass(string method, string absolutePath, string workPath)
        {
            this.workPath = workPath;
            this.absolutePath = absolutePath;
            this.method = method;
        }

        public string? ProcessRequest(byte[] PostData, string ContentType, string directoryPath)
        {
            if (string.IsNullOrEmpty(absolutePath) || string.IsNullOrEmpty(directoryPath))
                return null;

            string res = string.Empty;

            Directory.CreateDirectory(directoryPath);

            switch (method)
            {
                case "POST":
                    switch (absolutePath)
                    {
                        case "/capone/reportCollector/submit/":
                            {
                                string output = GriefReporter.caponeReportCollectorSubmit(PostData, ContentType, directoryPath);
                                return output;
                            }
                        case "/capone/contentStore/10/":
                            {
                                string output = GriefReporter.caponeContentStoreUpload(PostData, ContentType, directoryPath);
                                return output;
                            }
                    }
                    break;
                default:
                    break;
            }

            return res;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    absolutePath = string.Empty;
                    method = string.Empty;
                }

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~HOMECOREClass()
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
