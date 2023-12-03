namespace HTTPServer.API.JUGGERNAUT.farm.plant
{
    public class plant_getxp
    {
        public static string? ProcessGetXp(Dictionary<string, string>? QueryParameters)
        {
            if (QueryParameters != null)
            {
                string? user = QueryParameters["user"];

                if (!string.IsNullOrEmpty(user))
                {
                    Directory.CreateDirectory($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/farm/User_Data");

                    if (File.Exists($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/farm/User_Data/{user}.xml"))
                        return File.ReadAllText($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/farm/User_Data/{user}.xml");
                    else
                        return "<xml><found>0</found></xml>";
                }
            }

            return null;
        }
    }
}
