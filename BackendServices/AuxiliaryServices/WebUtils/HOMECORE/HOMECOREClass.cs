namespace WebUtils.HOMECORE
{
    public class HOMECOREClass : IDisposable
    {
        private string absolutepath;
        private string method;
        private bool disposedValue;

        public HOMECOREClass(string method, string absolutepath)
        {
            this.absolutepath = absolutepath;
            this.method = method;
        }

        public string? ProcessRequest(byte[] PostData, string? ContentType, string directoryPath)
        {
            if (string.IsNullOrEmpty(absolutepath) || string.IsNullOrEmpty(directoryPath))
                return null;

            string? res = null;

            Directory.CreateDirectory(directoryPath);

            switch (method)
            {
                case "GET":
                    switch (absolutepath)
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
                    absolutepath = string.Empty;
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
