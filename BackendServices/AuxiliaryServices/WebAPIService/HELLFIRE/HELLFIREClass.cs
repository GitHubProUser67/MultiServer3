using System;
using TycoonServer.HFProcessors;
using WebAPIService.HELLFIRE.Helpers;
using WebAPIService.HELLFIRE.HFProcessors;

namespace WebAPIService.HELLFIRE
{
    public class HELLFIREClass : IDisposable
    {
        private string workpath;
        private string absolutepath;
        private string method;
        private bool disposedValue;

        public HELLFIREClass(string method, string absolutepath, string workpath)
        {
            this.absolutepath = absolutepath;
            this.workpath = workpath;
            this.method = method;
        }

        public string? ProcessRequest(byte[] PostData, string ContentType)
        {
            if (string.IsNullOrEmpty(absolutepath))
                return null;

            switch (method)
            {
                case "POST":
                    switch (absolutepath)
                    {
                        #region HomeTycoon
                        case "/HomeTycoon/Main_SCEE.php":
                            return TycoonRequestProcessor.ProcessMainPHP(PostData, ContentType, null, workpath);
                        case "/HomeTycoon/Main_SCEJ.php":
                            return TycoonRequestProcessor.ProcessMainPHP(PostData, ContentType, null, workpath);
                        case "/HomeTycoon/Main_SCEAsia.php":
                            return TycoonRequestProcessor.ProcessMainPHP(PostData, ContentType, null, workpath);
                        case "/HomeTycoon/Main.php":
                            return TycoonRequestProcessor.ProcessMainPHP(PostData, ContentType, null, workpath);
                        #endregion

                        #region ClearasilSkater
                        case "/ClearasilSkater/Main.php":
                            return ClearasilSkaterRequestProcessor.ProcessMainPHP(PostData, ContentType, null, workpath);
                        #endregion

                        #region Novus Primus Prime
                        case "/Main.php":
                            //return NovusPrimeRequestProcessor.ProcessMainPHP(PostData, ContentType, null, workpath);
                        #endregion

                        #region Giftinator
                        case "/Giftinator/Main.php":
                            //return GiftinatorRequestProcessor.ProcessMainPHP(PostData, ContentType, null, workpath);
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
