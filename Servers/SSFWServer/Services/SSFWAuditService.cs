using CustomLogger;
using Newtonsoft.Json;
using System.Text;
using WebAPIService.SSFW;

namespace SSFWServer.Services
{
    public class SSFWAuditService : IDisposable
    {
        private string? sessionid;
        private string? env;
        private string? key;
        private bool disposedValue;

        public SSFWAuditService(string sessionid, string env, string? key)
        {
            this.sessionid = sessionid;
            this.env = env;
            this.key = key;
        }

        public void HandleAuditService(string absolutepath, byte[] buffer)
        {
            string fileNameGUID = GuidGenerator.SSFWGenerateGuid(sessionid, env);
            string auditLogPath = $"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutepath}/{env}";
            Directory.CreateDirectory(auditLogPath);

            var obj = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(buffer));
            File.WriteAllText($"{auditLogPath}/{fileNameGUID}.json", JsonConvert.SerializeObject(obj, Formatting.Indented));
            LoggerAccessor.LogInfo($"[SSFW] : Audit event posted: {fileNameGUID}");
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
