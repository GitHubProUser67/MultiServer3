namespace HTTPServer.API.JUGGERNAUT.cutteridge
{
    public class effects2012chances
    {
        public static string? ProcessChances()
        {
            if (File.Exists($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/cutteridge/effects2012chances.xml"))
                return File.ReadAllText($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/cutteridge/effects2012chances.xml");

            return null;
        }
    }
}
