namespace HTTPServer.API.JUGGERNAUT.farm.animal
{
    public class animal_stats
    {
        public static string? ProcessStats()
        {
            if (File.Exists($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/farm/animal_stats.xml"))
                return File.ReadAllText($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/farm/animal_stats.xml");

            return null;
        }
    }
}
