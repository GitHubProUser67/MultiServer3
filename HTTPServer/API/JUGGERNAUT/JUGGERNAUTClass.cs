namespace HTTPServer.API.JUGGERNAUT
{
    public class JUGGERNAUTClass : IDisposable
    {
        string absolutepath;
        string method;
        private bool disposedValue;

        public JUGGERNAUTClass(string method, string absolutepath)
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
                case "GET":
                    switch (absolutepath)
                    {
                        case "/clearasil/pushtelemetry.php":
                            if (QueryParameters != null)
                            {
                                string? user = QueryParameters["user"];
                                string? timeingame = QueryParameters["timeingame"];
                                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(timeingame))
                                {
                                    try
                                    {
                                        int time = int.Parse(timeingame);
                                        CustomLogger.LoggerAccessor.LogInfo($"[JUGGERNAUT] - User: {user} spent {time / 60}:{time % 60} minutes in clearasil.");
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }
                                return string.Empty;
                            }
                            break;
                        case "/clearasil/joinedspace.php":
                            return clearasil.joinedspace.ProcessJoinedSpace(QueryParameters);
                        case "/clearasil/getscores.php":
                            return clearasil.getscores.ProcessGetScores(QueryParameters);
                        case "/clearasil/pushrewards.php":
                            return clearasil.pushrewards.ProcessPushRewards(QueryParameters);
                        case "/clearasil/pushtime.php":
                            return clearasil.pushtime.ProcessPushTime(QueryParameters);
                        case "/clearasil/pushscore.php":
                            return clearasil.pushscore.ProcessPushScore(QueryParameters);
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
        // ~JUGGERNAUTClass()
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
