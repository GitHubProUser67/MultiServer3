namespace WebUtils.JUGGERNAUT.farm.crafting
{
    public class crafting_stats
    {
        public static string? ProcessGetStats(string apiPath)
        {
            if (File.Exists($"{apiPath}/juggernaut/farm/crafting_stats.xml"))
                return File.ReadAllText($"{apiPath}/juggernaut/farm/crafting_stats.xml");

            return null;
        }
    }
}
