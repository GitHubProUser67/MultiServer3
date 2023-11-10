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

        public string? ProcessRequest(byte[]? PostData, string? ContentType)
        {
            if (string.IsNullOrEmpty(absolutepath))
                return null;

            switch (method)
            {
                case "POST":
                    switch (absolutepath)
                    {
                        case "/commerce/get_count.php":
                            return Commerce.Get_Count();
                        case "/commerce/get_ownership.php":
                            return Commerce.Get_Ownership();
                        case "/data/parkChallenges.php":
                            return Data.ParkChallenges();
                        case "/data/parkTasks.php":
                            return Data.ParkTasks();
                        case "/slot-management/getobjectslot.php":
                            return Slot.GetObjectSlot(PostData, ContentType);
                        case "/slot-management/remove.php":
                            return Slot.RemoveSlot(PostData, ContentType);
                        case "/slot-management/heartbeat.php":
                            return Slot.HeartBeat(PostData, ContentType);
                        case "/stats/getconfig.php":
                            return Stats.GetConfig(false, PostData, ContentType);
                        case "/stats/crash.php":
                            return Stats.Crash(PostData, ContentType);
                        case "/stats_tracking/usage.php":
                            return Stats.Usage(PostData, ContentType);
                        case "/storage/readconfig.php":
                            return Storage.ReadConfig(PostData, ContentType);
                        case "/storage/readtable.php":
                            return Storage.ReadTable(PostData, ContentType);
                        case "/storage/writetable.php":
                            return Storage.WriteTable(PostData, ContentType);
                        default:
                            break;
                    }
                    break;
                case "GET":
                    switch (absolutepath)
                    {
                        case "/stats/getconfig.php":
                            return Stats.GetConfig(true, PostData, ContentType);
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
