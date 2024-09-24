using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CustomLogger;
using WebAPIService.NDREAMS.Aurora;
using WebAPIService.NDREAMS.BlueprintHome;
using WebAPIService.NDREAMS.Fubar;
using WebAPIService.NDREAMS.Xi2;

namespace WebAPIService.NDREAMS
{
    public partial class NDREAMSClass : IDisposable
    {
        private bool disposedValue;
        private DateTime currentdate;
        private string absolutepath;
        private string baseurl;
        private string filepath;
        private string fullurl;
        private string apipath;
        private string method;
        private string host;

        public NDREAMSClass(DateTime currentdate, string method, string filepath, string baseurl, string fullurl, string absolutepath, string apipath, string host)
        {
            this.currentdate = currentdate;
            this.absolutepath = absolutepath;
            this.filepath = filepath;
            this.baseurl = baseurl;
            this.fullurl = fullurl;
            this.method = method;
            this.apipath = apipath;
            this.host = host;
        }

        public string ProcessRequest(IDictionary<string, string> QueryParameters, byte[] PostData = null, string ContentType = null)
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
                            return Blimp.ProcessBlimps(currentdate, PostData, ContentType);
                        case "/aurora/almanac.php":
                        case "/aurora/almanacWeights.php":
                            return Almanac.ProcessAlmanac(currentdate, PostData, ContentType, fullurl, apipath);
                        case "/aurora/MysteryItems/mystery3.php":
                            return Mystery3.ProcessMystery3(currentdate, PostData, ContentType, fullurl, apipath);
                        case "/aurora/VisitCounter2.php":
                            return AuroraDBManager.ProcessVisitCounter2(currentdate, PostData, ContentType, apipath);
                        case "/aurora/TheEnd.php":
                            return AuroraDBManager.ProcessTheEnd(currentdate, PostData, ContentType, apipath);
                        case "/aurora/OrbrunnerScores.php":
                            return AuroraDBManager.ProcessOrbrunnerScores(currentdate, PostData, ContentType, apipath);
                        case "/aurora/Consumables.php":
                            return AuroraDBManager.ProcessConsumables(currentdate, PostData, ContentType, apipath);
                        case "/aurora/PStats.php":
                            return AuroraDBManager.ProcessPStats(PostData, ContentType);
                        case "/aurora/ReleaseInfo.php":
                            return AuroraDBManager.ProcessReleaseInfo(currentdate, PostData, ContentType, apipath);
                        case "/aurora/AuroraXP.php":
                            return AuroraDBManager.ProcessAuroraXP(currentdate, PostData, ContentType, apipath);
                        case "/aurora/VRSignUp.php":
                            return VRSignUp.ProcessVRSignUp(PostData, ContentType, apipath);
                        case "/xi2/cont/xi2_cont.php":
                            return Cont.ProcessCont(currentdate, PostData, ContentType, apipath);
                        case "/xi2/cont/battle_cont.php":
                            return BattleCont.ProcessBattleCont(currentdate, PostData, ContentType, apipath);
                        case "/xi2/cont/articles_cont.php":
                            return ArticlesCont.ProcessArticlesCont(currentdate, PostData, ContentType, apipath);
                        case "/xi2/cont/PStats.php":
                            return PStats.ProcessPStats(PostData, ContentType);
                        case "/gateway/":
                            return "<xml></xml>"; // Not gonna emulate this encrypted mess.
                        case "/thecomplex/ComplexABTest.php":
                            return AuroraDBManager.ProcessComplexABTest(currentdate, PostData, ContentType);
                        case "/blueprint/blueprint_furniture.php":
                            return Furniture.ProcessFurniture(currentdate, PostData, ContentType, baseurl, apipath);
                        default:
                            LoggerAccessor.LogWarn($"[NDREAMS] - Unknown POST method: {absolutepath} was requested. Please report to GITHUB");
                            break;
                    }
                    break;
                case "GET":
                    {
                        if (host.Equals("nDreams-multiserver-cdn"))
                        {
                            if (File.Exists(filepath)) // We do some api filtering afterwards.
                            {
                                if (filepath.Contains("/NDREAMS/BlueprintHome/Layout/"))
                                {
                                    try
                                    {
                                        // Split the URL into segments
                                        string[] segments = filepath.Trim('/').Split('/');

                                        if (segments.Length == 5) // Url is effectively a Blueprint Home Furn/Layout fetch, so we update current used slot for each.
                                        {
#if NET7_0_OR_GREATER
                                            Match match = BlueprintHomeRegex().Match(segments[4]);
#else
                                            Match match = Regex.Match(segments[4], @"blueprint_(\d+)\.xml");
#endif
                                            if (match.Success)
                                                File.WriteAllText(apipath + $"/NDREAMS/BlueprintHome/{segments[2]}/{segments[3]}/CurrentSlot.txt", match.Groups[1].Value);
                                            else
                                                LoggerAccessor.LogError($"[NDREAMS] - Server received an invalid BlueprintHome layout slot number!");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LoggerAccessor.LogError($"[NDREAMS] - Server thrown an exception while updating a BlueprintHome Current Slot: {ex}");
                                    }
                                }

                                return File.ReadAllText(filepath);
                            }
                            else
                                LoggerAccessor.LogWarn($"[NDREAMS] - Client requested a non-existant nDreams CDN file: {filepath}");
                        }
                        else
                            LoggerAccessor.LogWarn($"[NDREAMS] - Unknown GET method: {absolutepath} was requested. Please report to GITHUB");
                        break;
                    }
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
#if NET7_0_OR_GREATER
        [GeneratedRegex(@"blueprint_(\d+)\.xml")]
        private static partial Regex BlueprintHomeRegex();
#endif
    }
}
