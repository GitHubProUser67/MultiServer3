
namespace WebUtils.CDM
{
    public class CDMClass : IDisposable
    {
        private string workPath;
        private string absolutePath;
        private string method;
        private bool disposedValue;

        public CDMClass(string method, string absolutePath, string workPath)
        {
            this.workPath = workPath;
            this.absolutePath = absolutePath;
            this.method = method;
        }

        public string? ProcessRequest(byte[] PostData, string ContentType, string directoryPath)
        {
            if (string.IsNullOrEmpty(absolutePath) || string.IsNullOrEmpty(directoryPath))
                return null;

            string res = string.Empty;

            Directory.CreateDirectory(directoryPath);

            switch (method)
            {
                case "GET":
                    switch (absolutePath)
                    {
                        case "/publisher/list/":
                            return "<xml><status>success</status></xml>"; // TODO: emulate the publishers system.
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
                    absolutePath = string.Empty;
                    method = string.Empty;
                }

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~HOMECOREClass()
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
