namespace WebAPIService.LOOT
{
    public class LOOTClass : IDisposable
    {
        private bool disposedValue;
        private string absolutepath;
        private string method;

        public LOOTClass(string method, string absolutepath)
        {
            this.absolutepath = absolutepath;
            this.method = method;
        }

        public string? ProcessRequest(Dictionary<string, string>? QueryParameters, byte[]? PostData = null, string? ContentType = null)
        {
            if (string.IsNullOrEmpty(absolutepath))
                return null;

            switch (method)
            {
                case "POST":
                    switch (absolutepath)
                    {
                        case "/index.action.php":
                            if (PostData != null && !string.IsNullOrEmpty(ContentType))
                                return LOOTDatabase.ProcessDatabaseRequest(PostData, ContentType);
                            break;
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
        // ~LOOTClass()
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