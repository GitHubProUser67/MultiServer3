using WebAPIService.VEEMEE.goalie_sfrgbt;
using WebAPIService.VEEMEE.gofish;
using WebAPIService.VEEMEE.nml;
using WebAPIService.VEEMEE.olm;

namespace WebAPIService.VEEMEE
{
    public class VEEMEEClass : IDisposable
    {
        private string absolutepath;
        private string method;
        private bool disposedValue;

        public VEEMEEClass(string method, string absolutepath)
        {
            this.absolutepath = absolutepath;
            this.method = method;
        }

        public (string?, string?) ProcessRequest(byte[]? PostData, string? ContentType, string apiPath)
        {
            if (string.IsNullOrEmpty(absolutepath))
                return (null, null);

            switch (method)
            {
                case "POST":
                    switch (absolutepath)
                    {
                        case "/goalie/goalieHSSetUserData.php":
                            return (UserData.SetUserDataPOST(PostData, ContentType, true, apiPath), "text/xml");
                        case "/sfrgbt/sfrgbtHSSetUserData.php":
                            return (UserData.SetUserDataPOST(PostData, ContentType, false, apiPath), "text/xml");
                        case "/goali/goalieHSGetUserData.php":
                            return (UserData.GetUserDataPOST(PostData, ContentType, true, apiPath), "text/xml");
                        case "/sfrgbt/sfrgbtHSGetUserData.php":
                            return (UserData.GetUserDataPOST(PostData, ContentType, false, apiPath), "text/xml");
                        case "/goalie/goalieHSGetLeaderboard.php":
                            return (GSLeaderboard.GetLeaderboardPOST(PostData, ContentType, true, apiPath), "text/xml");
                        case "/sfrgbt/sfrgbtHSGetLeaderboard.php":
                            return (GSLeaderboard.GetLeaderboardPOST(PostData, ContentType, false, apiPath), "text/xml");
                        case "/gofish/goFishHSGetFishCaught.php":
                            return (FishCaughtProcess.GetPOST(PostData, ContentType, apiPath), "text/xml");
                        case "/gofish/goFishHSSetFishCaught.php":
                            return (FishCaughtProcess.SetPOST(PostData, ContentType, apiPath), "text/xml");
                        case "/gofish/goFishHSSetUserData.php":
                            return (GFUserData.SetUserDataPOST(PostData, ContentType, apiPath), "text/xml");
                        case "/gofish/goFishHSGetUserData.php":
                            return (GFUserData.GetUserDataPOST(PostData, ContentType, apiPath), "text/xml");
                        case "/gofish/goFishHSGetLeaderboard.php":
                            return (GFLeaderboard.GetLeaderboardPOST(PostData, ContentType, 2, apiPath), "text/xml");
                        case "/gofish/goFishHSGetLeaderboardToday.php":
                            return (GFLeaderboard.GetLeaderboardPOST(PostData, ContentType, 0, apiPath), "text/xml");
                        case "/gofish/goFishHSGetLeaderboardYesterday.php":
                            return (GFLeaderboard.GetLeaderboardPOST(PostData, ContentType, 1, apiPath), "text/xml");
                        case "/olm/olmHSSetUserData.php":
                            return (olmUserData.SetUserDataPOST(PostData, ContentType, apiPath), "text/xml");
                        case "/olm/olmHSGetUserData.php":
                            return (olmUserData.GetUserDataPOST(PostData, ContentType, apiPath), "text/xml");
                        case "/commerce/get_count.php":
                            return (Commerce.Get_Count(), null);
                        case "/commerce/get_ownership.php":
                            return (Commerce.Get_Ownership(), null);
                        case "/data/parkChallenges.php":
                            return (Data.ParkChallenges(apiPath), null);
                        case "/data/parkTasks.php":
                            return (Data.ParkTasks(apiPath), null);
                        case "/slot-management/getobjectspace.php":
                            return (Slot.GetObjectSpace(PostData, ContentType), null);
                        case "/slot-management/getobjectslot.php":
                            return (Slot.GetObjectSlot(PostData, ContentType), null);
                        case "/slot-management/remove.php":
                            return (Slot.RemoveSlot(PostData, ContentType), null);
                        case "/slot-management/heartbeat.php":
                            return (Slot.HeartBeat(PostData, ContentType), null);
                        case "/stats/getconfig.php":
                            return (Stats.GetConfig(false, PostData, ContentType, apiPath), null);
                        case "/stats/crash.php":
                            return (Stats.Crash(PostData, ContentType, apiPath), null);
                        case "/stats_tracking/usage.php":
                            return (Stats.Usage(PostData, ContentType), null);
                        case "/storage/readconfig.php":
                            return (Storage.ReadConfig(PostData, ContentType, apiPath), null);
                        case "/storage/readtable.php":
                            return (Storage.ReadTable(PostData, ContentType, apiPath), null);
                        case "/storage/writetable.php":
                            return (Storage.WriteTable(PostData, ContentType, apiPath), null);
                        case "/nml/rc2/profile/verify.php":
                            return (Profile.Verify(PostData, ContentType), null);
                        case "/nml/rc2/profile/reward.php":
                            return (Profile.Reward(PostData, ContentType), null);
                        case "/nml/rc2/profile/get.php":
                            return (Profile.Get(PostData, ContentType, apiPath), null);
                        case "/nml/rc2/profile/set.php":
                            return (Profile.Set(PostData, ContentType, apiPath), null);
                        case "/nml/rc2/stats/getConfig.php":
                            return (Stats.GetConfig(false, PostData, ContentType, apiPath), null);
                        default:
                            break;
                    }
                    break;
                case "GET":
                    switch (absolutepath)
                    {
                        case "/stats/getconfig.php":
                            return (Stats.GetConfig(true, PostData, ContentType, apiPath), null);
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            return (null, null);
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
        // ~Class()
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
