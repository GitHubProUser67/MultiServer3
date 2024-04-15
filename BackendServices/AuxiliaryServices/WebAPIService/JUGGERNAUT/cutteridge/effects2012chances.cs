namespace WebAPIService.JUGGERNAUT.cutteridge
{
    public class effects2012chances
    {
        public static string? ProcessChances(string apiPath)
        {
            if (File.Exists($"{apiPath}/juggernaut/cutteridge/effects2012chances.xml"))
                return File.ReadAllText($"{apiPath}/juggernaut/cutteridge/effects2012chances.xml");

            return null;
        }
    }
}
