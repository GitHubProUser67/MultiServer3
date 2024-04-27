using System;
using System.IO;

namespace WebAPIService.FROMSOFTWARE
{
    public class FROMSOFTWAREClass : IDisposable
    {
        private string absolutepath;
        private string method;
        private string apipath;
        private bool disposedValue;

        public FROMSOFTWAREClass(string method, string absolutepath, string apipath)
        {
            this.absolutepath = absolutepath;
            this.method = method;
            this.apipath = apipath;
        }

        public (byte[]?, string?, string[][]?) ProcessRequest(byte[]? PostData, string? ContentType)
        {
            if (string.IsNullOrEmpty(absolutepath))
                return (null, null, null);

            switch (method)
            {
                case "GET":
                    switch (absolutepath)
                    {
                        case "/regulation/contents_101.bin":
                            if (File.Exists(apipath + "/FROMSOFTWARE/regulation/contents_101.bin"))
                                return (File.ReadAllBytes(apipath + "/FROMSOFTWARE/regulation/contents_101.bin"), "application/octet-stream",
                                    new string[][] {
                                    new string[] { "Last-Modified", "Wed, 15 Jan 2014 08:12:11 GMT" },
                                    new string[] { "Accept-Ranges", "bytes" },
                                    new string[] { "X-Cache", "Hit from cloudfront" }, 
                                    new string[] { "Via", "1.1 6895284e395204317ac1aa2c7b0a3d0c.cloudfront.net (CloudFront)" },
                                    new string[] { "X-Amz-Cf-Pop", "MIA3-P4" },
                                    new string[] { "X-Amz-Cf-Id", "IefO3gqiGGVLwgLqePmDnindcBcuTqmbYD_Kp2GrZAsEqvtqes4qCg==" },
                                    new string[] { "Age", "18139" }});
                            break;
                    }
                    break;
                default:
                    break;
            }

            return (null, null, null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: supprimer l'état managé (objets managés)
                }

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~FROMSOFTWAREClass()
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
