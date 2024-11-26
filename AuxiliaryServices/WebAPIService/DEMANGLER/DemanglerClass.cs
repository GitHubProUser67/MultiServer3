using CustomLogger;
using System.Collections.Generic;
using System.Text;

namespace WebAPIService.DEMANGLER
{
    public class DemanglerClass
    {
        public static (string, string)? ProcessDemanglerRequest(IDictionary<string, string> QueryParameters, string absolutepath, string clientip, byte[] PostData)
        {
            switch (absolutepath)
            {
                case "/getPeerAddress":
                    if (QueryParameters.Count > 0 && QueryParameters.ContainsKey("myPort"))
                        return ($"status=success\n\n\npeerIP={clientip}\npeerPort={QueryParameters["myPort"]}\n", "text/plain;charset=UTF-8");
                    break;
                case "/connectionStatus":
                    string connectionInfos = Encoding.UTF8.GetString(PostData);
                    LoggerAccessor.LogWarn($"[DemanglerClass] - connectionStatus was sent, details: {connectionInfos}");
                    return (connectionInfos, "application/x-www-form-urlencoded");
            }

            return null;
        }
    }
}
