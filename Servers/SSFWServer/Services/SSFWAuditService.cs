using CustomLogger;
using Newtonsoft.Json;
using System.Text;
using WebAPIService.SSFW;

namespace SSFWServer.Services
{
    public class SSFWAuditService
    {
        private string? sessionid;
        private string? env;
        private string? key;

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

    }
}
