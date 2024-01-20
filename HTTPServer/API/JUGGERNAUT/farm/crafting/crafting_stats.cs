namespace HTTPServer.API.JUGGERNAUT.farm.crafting
{
    public class crafting_stats
    {
        public static string? ProcessGetStats()
        {
            if (File.Exists($"{HTTPServerConfiguration.APIStaticFolder}/juggernaut/farm/crafting_stats.xml"))
                return File.ReadAllText($"{HTTPServerConfiguration.APIStaticFolder}/juggernaut/farm/crafting_stats.xml");

            return null;
        }
    }
}
