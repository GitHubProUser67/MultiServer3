namespace HTTPSecureServerLite.API.VEEMEE
{
    public class VEEMEEClass : IDisposable
    {
        string absolutepath;
        string method;
        private bool disposedValue;

        public VEEMEEClass(string method, string absolutepath)
        {
            this.absolutepath = absolutepath;
            this.method = method;
        }

        public (string?, string?) ProcessRequest(byte[]? PostData, string? ContentType)
        {
            if (string.IsNullOrEmpty(absolutepath))
                return (null, null);

            switch (method)
            {
                case "POST":
                    switch (absolutepath)
                    {
                        case "/goalie/goalieHSSetUserData.php":
                            return (goalie_sfrgbt.UserData.SetUserDataPOST(PostData, ContentType, true), "text/xml");
                        case "/sfrgbt/sfrgbtHSSetUserData.php":
                            return (goalie_sfrgbt.UserData.SetUserDataPOST(PostData, ContentType, false), "text/xml");
                        case "/goalie/goalieHSGetUserData.php":
                            return (goalie_sfrgbt.UserData.GetUserDataPOST(PostData, ContentType, true), "text/xml");
                        case "/sfrgbt/sfrgbtHSGetUserData.php":
                            return (goalie_sfrgbt.UserData.GetUserDataPOST(PostData, ContentType, false), "text/xml");
                        case "/goalie/goalieHSGetLeaderboard.php":
                            return (goalie_sfrgbt.Leaderboard.GetLeaderboardPOST(PostData, ContentType, true), "text/xml");
                        case "/sfrgbt/sfrgbtHSGetLeaderboard.php":
                            return (goalie_sfrgbt.Leaderboard.GetLeaderboardPOST(PostData, ContentType, false), "text/xml");
                        case "/gofish/goFishHSGetFishCaught.php":
                            return (gofish.FishCaughtProcess.GetPOST(PostData, ContentType), "text/xml");
                        case "/gofish/goFishHSSetFishCaught.php":
                            return (gofish.FishCaughtProcess.SetPOST(PostData, ContentType), "text/xml");
                        case "/gofish/goFishHSSetUserData.php":
                            return (gofish.UserData.SetUserDataPOST(PostData, ContentType), "text/xml");
                        case "/gofish/goFishHSGetUserData.php":
                            return (gofish.UserData.GetUserDataPOST(PostData, ContentType), "text/xml");
                        case "/gofish/goFishHSGetLeaderboard.php":
                            return (gofish.Leaderboard.GetLeaderboardPOST(PostData, ContentType, 2), "text/xml");
                        case "/gofish/goFishHSGetLeaderboardToday.php":
                            return (gofish.Leaderboard.GetLeaderboardPOST(PostData, ContentType, 0), "text/xml");
                        case "/gofish/goFishHSGetLeaderboardYesterday.php":
                            return (gofish.Leaderboard.GetLeaderboardPOST(PostData, ContentType, 1), "text/xml");
                        case "/olm/olmHSSetUserData.php":
                            return (olm.UserData.SetUserDataPOST(PostData, ContentType), "text/xml");
                        case "/olm/olmHSGetUserData.php":
                            return (olm.UserData.GetUserDataPOST(PostData, ContentType), "text/xml");
                        case "/commerce/get_count.php":
                            return (Commerce.Get_Count(), null);
                        case "/commerce/get_ownership.php":
                            return (Commerce.Get_Ownership(), null);
                        case "/data/parkChallenges.php":
                            return (Data.ParkChallenges(), null);
                        case "/data/parkTasks.php":
                            return (Data.ParkTasks(), null);
                        case "/slot-management/getobjectslot.php":
                            return (Slot.GetObjectSlot(PostData, ContentType), null);
                        case "/slot-management/remove.php":
                            return (Slot.RemoveSlot(PostData, ContentType), null);
                        case "/slot-management/heartbeat.php":
                            return (Slot.HeartBeat(PostData, ContentType), null);
                        case "/stats/getconfig.php":
                            return (Stats.GetConfig(false, PostData, ContentType), null);
                        case "/stats/crash.php":
                            return (Stats.Crash(PostData, ContentType), null);
                        case "/stats_tracking/usage.php":
                            return (Stats.Usage(PostData, ContentType), null);
                        case "/storage/readconfig.php":
                            return (Storage.ReadConfig(PostData, ContentType), null);
                        case "/storage/readtable.php":
                            return (Storage.ReadTable(PostData, ContentType), null);
                        case "/storage/writetable.php":
                            return (Storage.WriteTable(PostData, ContentType), null);
                        default:
                            break;
                    }
                    break;
                case "GET":
                    switch (absolutepath)
                    {
                        case "/stats/getconfig.php":
                            return (Stats.GetConfig(true, PostData, ContentType), null);
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
