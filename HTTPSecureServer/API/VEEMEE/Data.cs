namespace HTTPSecureServer.API.VEEMEE
{
    public class Data
    {
        public static string? ParkChallenges()
        {
            if (File.Exists($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/Acorn_Medow/challenges.json"))
                return Processor.sign(File.ReadAllText($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/Acorn_Medow/challenges.json"));
            else
                return null;
        }

        public static string? ParkTasks()
        {
            if (File.Exists($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/Acorn_Medow/tasks.json"))
                return Processor.sign(File.ReadAllText($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/Acorn_Medow/tasks.json"));
            else
                return null;
        }
    }
}
