using System;
using System.Text;
using WebAPIService.LeaderboardsService.VEEMEE;
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

        public (byte[], string) ProcessRequest(byte[] postData, string contentType, string apiPath)
        {
            if (string.IsNullOrEmpty(apiPath))
                return (null, null);

            string result = null;
            string resultContentType = null;

            switch (method)
            {
                case "POST":
                    switch (absolutepath)
                    {
                        case "/goalie/goalieHSSetUserData.php":
                            result = UserData.SetUserDataPOST(postData, contentType, true, apiPath);
                            resultContentType = "text/xml";
                            break;
                        case "/sfrgbt/sfrgbtHSSetUserData.php":
                            result = UserData.SetUserDataPOST(postData, contentType, false, apiPath);
                            resultContentType = "text/xml";
                            break;
                        case "/goali/goalieHSGetUserData.php":
                            result = UserData.GetUserDataPOST(postData, contentType, true, apiPath);
                            resultContentType = "text/xml";
                            break;
                        case "/sfrgbt/sfrgbtHSGetUserData.php":
                            result = UserData.GetUserDataPOST(postData, contentType, false, apiPath);
                            resultContentType = "text/xml";
                            break;
                        case "/goalie/goalieHSGetLeaderboard.php":
                            result = GSLeaderboard.GetLeaderboardPOST(postData, contentType, true, apiPath);
                            resultContentType = "text/xml";
                            break;
                        case "/sfrgbt/sfrgbtHSGetLeaderboard.php":
                            result = GSLeaderboard.GetLeaderboardPOST(postData, contentType, false, apiPath);
                            resultContentType = "text/xml";
                            break;
                        case "/gofish/goFishHSGetFishCaught.php":
                            result = FishCaughtProcess.GetPOST(postData, contentType, apiPath);
                            resultContentType = "text/xml";
                            break;
                        case "/gofish/goFishHSSetFishCaught.php":
                            result = FishCaughtProcess.SetPOST(postData, contentType, apiPath);
                            resultContentType = "text/xml";
                            break;
                        case "/gofish/goFishHSSetUserData.php":
                            result = GFUserData.SetUserDataPOST(postData, contentType, apiPath);
                            resultContentType = "text/xml";
                            break;
                        case "/gofish/goFishHSGetUserData.php":
                            result = GFUserData.GetUserDataPOST(postData, contentType, apiPath);
                            resultContentType = "text/xml";
                            break;
                        case "/gofish/goFishHSGetLeaderboard.php":
                            result = GFLeaderboard.GetLeaderboardPOST(postData, contentType, 2, apiPath);
                            resultContentType = "text/xml";
                            break;
                        case "/gofish/goFishHSGetLeaderboardToday.php":
                            result = GFLeaderboard.GetLeaderboardPOST(postData, contentType, 0, apiPath);
                            resultContentType = "text/xml";
                            break;
                        case "/gofish/goFishHSGetLeaderboardYesterday.php":
                            result = GFLeaderboard.GetLeaderboardPOST(postData, contentType, 1, apiPath);
                            resultContentType = "text/xml";
                            break;
                        case "/olm/olmHSSetUserData.php":
                            result = olmUserData.SetUserDataPOST(postData, contentType, apiPath);
                            resultContentType = "text/xml";
                            break;
                        case "/olm/olmHSGetUserData.php":
                            result = olmUserData.GetUserDataPOST(postData, contentType, apiPath);
                            resultContentType = "text/xml";
                            break;
                        case "/commerce/get_count.php":
                            result = Commerce.Get_Count();
                            break;
                        case "/commerce/get_ownership.php":
                            result = Commerce.Get_Ownership();
                            break;
                        case "/data/parkChallenges.php":
                            result = Data.ParkChallenges(apiPath);
                            break;
                        case "/data/parkTasks.php":
                            result = Data.ParkTasks(apiPath);
                            break;
                        case "/slot-management/getobjectspace.php":
                            result = Slot.GetObjectSpace(postData, contentType);
                            break;
                        case "/slot-management/getobjectslot.php":
                            result = Slot.GetObjectSlot(postData, contentType);
                            break;
                        case "/slot-management/remove.php":
                            result = Slot.RemoveSlot(postData, contentType);
                            break;
                        case "/slot-management/heartbeat.php":
                            result = Slot.HeartBeat(postData, contentType);
                            break;
                        case "/stats/getconfig.php":
                            result = Stats.GetConfig(false, postData, contentType, apiPath);
                            break;
                        case "/stats/crash.php":
                            result = Stats.Crash(postData, contentType, apiPath);
                            break;
                        case "/stats_tracking/usage.php":
                            result = Stats.Usage(postData, contentType);
                            break;
                        case "/storage/readconfig.php":
                            result = Storage.ReadConfig(postData, contentType, apiPath);
                            break;
                        case "/storage/readtable.php":
                            result = Storage.ReadTable(postData, contentType, apiPath);
                            break;
                        case "/storage/writetable.php":
                            result = Storage.WriteTable(postData, contentType, apiPath);
                            break;
                        case "/nml/rc2/profile/verify.php":
                            result = Profile.Verify(postData, contentType);
                            break;
                        case "/nml/rc2/profile/reward.php":
                            result = Profile.Reward(postData, contentType);
                            break;
                        case "/nml/rc2/profile/get.php":
                            result = Profile.Get(postData, contentType, apiPath);
                            break;
                        case "/nml/rc2/profile/set.php":
                            result = Profile.Set(postData, contentType, apiPath);
                            break;
                        case "/nml/rc2/stats/getConfig.php":
                            result = Stats.GetConfig(false, postData, contentType, apiPath);
                            break;
                        case "/audi_tech/getprofile.php":
                            result = audi_tech.Profile.GetProfile(postData, contentType, apiPath);
                            break;
                        case "/audi_tech/setprofile.php":
                            result = audi_tech.Profile.SetProfile(postData, contentType, apiPath);
                            break;
                        case "/audi_tech/getFriendsGhostTimes.php":
                            result = audi_tech.Ghost.getFriendsGhostTimes(postData, contentType, apiPath);
                            break;
                        case "/audi_tech/getghost.php":
                            return (audi_tech.Ghost.getGhost(postData, contentType, apiPath), null);
                        default:
                            break;
                    }
                    break;
                case "GET":
                    switch (absolutepath)
                    {
                        case "/audi_tech/hiscores.xml":
                            result = audi_tech.Scoreboard.MockedScoreBoard; // TODO: implement real scoreboard.
                            resultContentType = "text/xml";
                            break;
                        case "/stats/getconfig.php":
                            result = Stats.GetConfig(true, postData, contentType, apiPath);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(result))
                return (Encoding.UTF8.GetBytes(result), resultContentType);

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
