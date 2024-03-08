namespace WebUtils.JUGGERNAUT.farm.plant
{
    public class plant_stats
    {
        public static string? ProcessStats(string apiPath)
        {
            if (File.Exists($"{apiPath}/juggernaut/farm/plant_stats.xml"))
                return File.ReadAllText($"{apiPath}/juggernaut/farm/plant_stats.xml");

            return null;
        }
    }
}
