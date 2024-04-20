using System;
using System.Collections.Generic;
using CustomLogger;
using WebAPIService.NDREAMS.Aurora;
using WebAPIService.NDREAMS.Fubar;

namespace WebAPIService.NDREAMS
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
                        case "/Teaser/beans.php":
                            return Teaser.ProcessBeans(PostData, ContentType);
                        case "/aurora/visit.php":
                            return visit.ProcessVisit(PostData, ContentType, apipath);
                        case "/aurora/Blimp.php":
                            return Blimp.ProcessBlimps(PostData, ContentType);
                        case "/aurora/almanac.php":
                        case "/aurora/almanacWeights.php":
                            return Almanac.ProcessAlmanac(PostData, ContentType, fullurl, apipath);
                        case "/aurora/MysteryItems/mystery3.php":
                            return Mystery3.ProcessMystery3(PostData, ContentType, fullurl, apipath);
                        case "/aurora/VisitCounter2.php":
                            return AuroraDBManager.ProcessVisitCounter2(PostData, ContentType, apipath);
                        case "/aurora/TheEnd.php":
                            return AuroraDBManager.ProcessTheEnd(PostData, ContentType, apipath);
                        case "/aurora/OrbrunnerScores.php":
                            return AuroraDBManager.ProcessOrbrunnerScores(PostData, ContentType, apipath);
                        case "/aurora/Consumables.php":
                            return AuroraDBManager.ProcessConsumables(PostData, ContentType, apipath);
                        case "/aurora/PStats.php":
                            return AuroraDBManager.ProcessPStats(PostData, ContentType);
                        case "/aurora/ReleaseInfo.php":
                            return AuroraDBManager.ProcessReleaseInfo(PostData, ContentType, apipath);
                        case "/aurora/AuroraXP.php":
                            return AuroraDBManager.ProcessAuroraXP(PostData, ContentType, apipath);
                        case "/aurora/VRSignUp.php":
                            return VRSignUp.ProcessVRSignUp(PostData, ContentType, apipath);
                        case "/gateway/":
                            return "<xml></xml>"; // Not gonna emulate this encrypted mess.
                        case "/thecomplex/ComplexABTest.php":
                            return AuroraDBManager.ProcessComplexABTest(PostData, ContentType);
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
