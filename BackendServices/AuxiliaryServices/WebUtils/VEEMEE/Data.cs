namespace WebUtils.VEEMEE
{
    public class Data
    {
        public static string? ParkChallenges(string apiPath)
        {
            if (File.Exists($"{apiPath}/VEEMEE/Acorn_Medow/challenges.json"))
                return Processor.sign(File.ReadAllText($"{apiPath}/VEEMEE/Acorn_Medow/challenges.json"));
            else
                return null;
        }

        public static string? ParkTasks(string apiPath)
        {
            if (File.Exists($"{apiPath}/VEEMEE/Acorn_Medow/tasks.json"))
                return Processor.sign(File.ReadAllText($"{apiPath}/VEEMEE/Acorn_Medow/tasks.json"));
            else
                return null;
        }
    }
}
