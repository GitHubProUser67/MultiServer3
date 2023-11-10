namespace HTTPSecureServerLite.API.HELLFIRE
{
    public class HELLFIREClass : IDisposable
    {
        string absolutepath;
        string method;
        private bool disposedValue;

        public HELLFIREClass(string method, string absolutepath)
        {
            this.absolutepath = absolutepath;
            this.method = method;
        }

        public string? ProcessRequest(byte[] PostData, string ContentType)
        {
            if (string.IsNullOrEmpty(absolutepath))
                return null;

            switch (method)
            {
                case "POST":
                    switch (absolutepath)
                    {
                        case "/HomeTycoon/Main_SCEE.php":
                            return Redirector.ProcessMainRedirector(PostData, ContentType);
                        case "/HomeTycoon/Main_SCEJ.php":
                            return Redirector.ProcessMainRedirector(PostData, ContentType);
                        case "/HomeTycoon/Main_SCEAsia.php":
                            return Redirector.ProcessMainRedirector(PostData, ContentType);
                        case "/HomeTycoon/Main.php":
                            return Redirector.ProcessMainRedirector(PostData, ContentType);
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
        // ~HELLFIREClass()
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
