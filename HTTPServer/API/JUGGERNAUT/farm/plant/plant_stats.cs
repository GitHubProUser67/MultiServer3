namespace HTTPServer.API.JUGGERNAUT.farm.plant
{
    public class plant_stats
    {
        public static string? ProcessStats()
        {
            if (File.Exists($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/farm/plant_stats.xml"))
                return File.ReadAllText($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/farm/plant_stats.xml");

            return null;
        }
    }
}
