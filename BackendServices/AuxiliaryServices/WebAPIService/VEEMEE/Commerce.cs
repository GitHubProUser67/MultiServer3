namespace WebAPIService.VEEMEE
{
    public class Commerce
    {
        public static string? Get_Count()
        {
            VEEMEELoginCounter? counter = new();
            string? returnstring = Processor.sign($"{{\"count\":{counter.GetLoginCount("Voodooperson05")}}}");
            counter = null;
            return returnstring;
        }

        public static string? Get_Ownership()
        {
            return Processor.sign("{\"owner\":\"Voodooperson05\"}");
        }

        private class VEEMEELoginCounter
        {
            private Dictionary<string, int> loginCounts;

            public VEEMEELoginCounter()
            {
                loginCounts = new Dictionary<string, int>();
            }

            public void ProcessLogin(string username)
            {
                if (loginCounts.TryGetValue(username, out int value))
                    loginCounts[username] = ++value;
                else
                    loginCounts.Add(username, 1);
            }

            public int GetLoginCount(string username)
            {
                if (loginCounts.TryGetValue(username, out int value))
                    return value;

                return 0;
            }
        }
    }
}
