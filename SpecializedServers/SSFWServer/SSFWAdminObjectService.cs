using CustomLogger;
using Newtonsoft.Json;
using BackendProject.FileHelper;

namespace SSFWServer
{
    public class SSFWAdminObjectService : IDisposable
    {
        private string? sessionid;
        private string? key;
        private bool disposedValue;

        public SSFWAdminObjectService(string sessionid, string? key)
        {
            this.sessionid = sessionid;
            this.key = key;
        }

        public bool HandleAdminObjectService(string UserAgent)
        {
            string? username = SSFWUserSessionManager.GetUsernameBySessionId(sessionid ?? string.Empty);

            if (!string.IsNullOrEmpty(username) && File.Exists($"{SSFWServerConfiguration.SSFWStaticFolder}/SSFW_Accounts/{username}.json"))
            {
                string? userprofiledata = FileHelper.ReadAllText($"{SSFWServerConfiguration.SSFWStaticFolder}/SSFW_Accounts/{username}.json", key);

                if (!string.IsNullOrEmpty(userprofiledata))
                {
                    // Parsing JSON data to SSFWUserData object
                    SSFWUserData? userData = JsonConvert.DeserializeObject<SSFWUserData>(userprofiledata);

                    if (userData != null)
                    {
                        LoggerAccessor.LogInfo($"[SSFW] - IGA Request from : {UserAgent}/{username} - IGA status : {userData.IGA}");

                        if (userData.IGA == 1)
                        {
                            LoggerAccessor.LogInfo($"[SSFW] - Admin role confirmed for : {UserAgent}/{username}");

                            return true;
                        }
                    }
                }
            }

            LoggerAccessor.LogError($"[SSFW] - {UserAgent} requested a IGA access, but no access allowed so we forbid!");

            return false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    sessionid = null;
                    key = null;
                }

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~SSFWAdminObjectService()
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
