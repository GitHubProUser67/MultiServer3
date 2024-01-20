namespace HTTPServer.API.JUGGERNAUT.clearasil
{
    public class getscores
    {
        public static string? ProcessGetScores(Dictionary<string, string>? QueryParameters)
        {
            if (QueryParameters != null)
            {
                string? phase = QueryParameters["phase"];

                if (!string.IsNullOrEmpty(phase))
                {
                    Directory.CreateDirectory($"{HTTPServerConfiguration.APIStaticFolder}/juggernaut/clearasil");

                    if (File.Exists($"{HTTPServerConfiguration.APIStaticFolder}/juggernaut/clearasil/scoreboard.xml"))
                        return File.ReadAllText($"{HTTPServerConfiguration.APIStaticFolder}/juggernaut/clearasil/scoreboard.xml");
                    else
                        return "<xml></xml>";
                }
            }

            return null;
        }
    }
}
