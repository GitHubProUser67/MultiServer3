using System.IO;
namespace WebAPIService.JUGGERNAUT.farm.animal
{
    public class animal_stats
    {
        public static string? ProcessStats(string apiPath)
        {
            if (File.Exists($"{apiPath}/juggernaut/farm/animal_stats.xml"))
                return File.ReadAllText($"{apiPath}/juggernaut/farm/animal_stats.xml");

            return null;
        }
    }
}
