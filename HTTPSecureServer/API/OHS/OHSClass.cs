namespace HTTPSecureServer.API.OHS
{
    public class OHSClass : IDisposable
    {
        string absolutepath;
        string method;
        private bool disposedValue;

        public OHSClass(string method, string absolutepath)
        {
            this.absolutepath = absolutepath;
            this.method = method;
        }

        public string? ProcessRequest(byte[] PostData, string? ContentType, string directoryPath)
        {
            if (string.IsNullOrEmpty(absolutepath))
                return null;

            string? res = null;

            Directory.CreateDirectory(directoryPath);

            switch (method)
            {
                case "POST":
                    if (ContentType != null && ContentType.Contains("multipart/form-data"))
                    {
                        if (absolutepath.Contains("/batch/"))
                            res = Batch.Batch_Process(PostData, ContentType, directoryPath);
                        else if (absolutepath.Contains("community/getscore/"))
                            res = Community.Community_Getscore(PostData, ContentType, directoryPath, string.Empty);
                        else if (absolutepath.Contains("community/updatescore/"))
                            res = Community.Community_UpdateScore(PostData, ContentType, directoryPath, string.Empty);
                        else if (absolutepath.Contains("global/set/"))
                            res = User.Set(PostData, ContentType, directoryPath, string.Empty, true);
                        else if (absolutepath.Contains("global/getall/"))
                            res = User.Get_All(PostData, ContentType, directoryPath, string.Empty, true);
                        else if (absolutepath.Contains("global/get/"))
                            res = User.Get(PostData, ContentType, directoryPath, string.Empty, true);
                        else if (absolutepath.Contains("user/getwritekey/"))
                            res = User.User_GetWritekey(PostData, ContentType, string.Empty);
                        else if (absolutepath.Contains("user/set/"))
                            res = User.Set(PostData, ContentType, directoryPath, string.Empty, false);
                        else if (absolutepath.Contains("user/getall/"))
                            res = User.Get_All(PostData, ContentType, directoryPath, string.Empty, false);
                        else if (absolutepath.Contains("user/get/"))
                            res = User.Get(PostData, ContentType, directoryPath, string.Empty, false);
                        else if (absolutepath.Contains("usercounter/set/"))
                            res = UserCounter.Set(PostData, ContentType, directoryPath, string.Empty);
                        else if (absolutepath.Contains("usercounter/getall/"))
                            res = UserCounter.Get_All(PostData, ContentType, directoryPath, string.Empty);
                        else if (absolutepath.Contains("usercounter/get/"))
                            res = UserCounter.Get(PostData, ContentType, directoryPath, string.Empty);
                        else if (absolutepath.Contains("userinventory/getglobalitems/"))
                            res = UserInventory.GetGlobalItems(PostData, ContentType, directoryPath, string.Empty);
                        else if (absolutepath.Contains("userinventory/getuserinventory/"))
                            res = UserInventory.GetUserInventory(PostData, ContentType, directoryPath, string.Empty);
                        else if (absolutepath.Contains("leaderboard/requestbyusers/"))
                            res = Leaderboard.Leaderboard_RequestByUsers(PostData, ContentType, directoryPath, string.Empty);
                        else if (absolutepath.Contains("leaderboard/requestbyrank/"))
                            res = Leaderboard.Leaderboard_RequestByRank(PostData, ContentType, directoryPath, string.Empty);
                        else if (absolutepath.Contains("leaderboard/update/"))
                            res = Leaderboard.Leaderboard_Update(PostData, ContentType, directoryPath, string.Empty);
                        else if (absolutepath.Contains("leaderboard/updatessameentry/"))
                            res = Leaderboard.Leaderboard_UpdatesSameEntry(PostData, ContentType, directoryPath, string.Empty);
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
                    absolutepath = string.Empty;
                    method = string.Empty;
                }

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~OHSClass()
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
