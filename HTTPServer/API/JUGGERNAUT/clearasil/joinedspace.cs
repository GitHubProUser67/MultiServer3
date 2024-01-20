namespace HTTPServer.API.JUGGERNAUT.clearasil
{
    public class joinedspace
    {
        public static string? ProcessJoinedSpace(Dictionary<string, string>? QueryParameters)
        {
            if (QueryParameters != null)
            {
                string? user = QueryParameters["user"];

                if (!string.IsNullOrEmpty(user))
                {
                    Directory.CreateDirectory($"{HTTPServerConfiguration.APIStaticFolder}/juggernaut/clearasil/space_access");

                    if (File.Exists($"{HTTPServerConfiguration.APIStaticFolder}/juggernaut/clearasil/space_access/{user}.xml"))
                        return File.ReadAllText($"{HTTPServerConfiguration.APIStaticFolder}/juggernaut/clearasil/space_access/{user}.xml");
                    else
                    {
                        string XmlData = "<xml><seconds>500</seconds><phase2>0</phase2><score>0</score></xml>";
                        File.WriteAllText($"{HTTPServerConfiguration.APIStaticFolder}/juggernaut/clearasil/space_access/{user}.xml", XmlData);
                        return XmlData;
                    }
                }
            }

            return null;
        }
    }
}
