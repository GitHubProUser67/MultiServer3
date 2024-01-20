namespace HTTPServer.API.JUGGERNAUT.farm
{
    public class resources_getall
    {
        public static string? ProcessGetAll(Dictionary<string, string>? QueryParameters)
        {
            if (QueryParameters != null)
            {
                string? user = QueryParameters["user"];

                if (!string.IsNullOrEmpty(user))
                {
                    Directory.CreateDirectory($"{HTTPServerConfiguration.APIStaticFolder}/juggernaut/farm/User_Data");

                    if (File.Exists($"{HTTPServerConfiguration.APIStaticFolder}/juggernaut/farm/User_Data/{user}.xml"))
                        return File.ReadAllText($"{HTTPServerConfiguration.APIStaticFolder}/juggernaut/farm/User_Data/{user}.xml");
                    else
                    {
                        File.WriteAllText($"{HTTPServerConfiguration.APIStaticFolder}/juggernaut/farm/User_Data/{user}.xml",
                                $"<xml><found>1</found><resources><wood>0</wood><gold>0</gold><weather>0</weather><level>0</level>" +
                                "<xp>0</xp><lastLayout>0</lastLayout><remodel>1</remodel></resources><animals></animals><plants></plants></xml>");

                        return "<xml><found>0</found></xml>";
                    }
                }
            }

            return null;
        }
    }
}
