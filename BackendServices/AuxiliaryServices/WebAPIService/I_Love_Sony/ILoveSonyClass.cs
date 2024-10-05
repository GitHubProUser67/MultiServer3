using NetworkLibrary.HTTP;
using System;
using WebAPIService.HTS.Helpers;

namespace WebAPIService.ILoveSony
{
    public class ILoveSonyClass : IDisposable
    {
        private string workpath;
        private string absolutepath;
        private string method;
        private bool disposedValue;

        public ILoveSonyClass(string method, string absolutepath, string workpath)
        {
            this.absolutepath = absolutepath;
            this.workpath = workpath;
            this.method = method;
        }

        public string? ProcessRequest(byte[] PostData, string ContentType, bool https)
        {
            if (string.IsNullOrEmpty(absolutepath))
                return null;

            switch (method)
            {
                case "GET":
                    switch (absolutepath)
                    {

                        #region Resistance Fall of Man EULA
                        case "/i_love_sony/legal/UP9000-BCUS98107_00/1":
                            return MyResistanceEula.ILoveSonyEula(PostData, HTTPProcessor.ExtractBoundary(ContentType));
                        #endregion

                        default:    
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
        // ~HELLFIREClass()
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
